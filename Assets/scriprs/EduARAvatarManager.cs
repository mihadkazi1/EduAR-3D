using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Local profile-photo picker/storage for EduAR 3D.
/// Requires the NativeGallery plugin by yasirkula.
/// </summary>
public static class EduARAvatarManager
{
    private const int MaxAvatarSize = 512;

    public static bool HasAvatar(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }

    public static Texture2D LoadAvatarTexture(string path)
    {
        if (!HasAvatar(path))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0)
                return null;

            // Keep the displayed texture usable by Sprite.Create. It does not
            // need to remain CPU-readable after this load.
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes, true))
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            return texture;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[EduAR] Avatar load failed: " + ex.Message);
            return null;
        }
    }

    public static void PickAndStoreAvatar(
        string userId,
        Action<string> onComplete)
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(userId))
        {
            onComplete?.Invoke(null);
            return;
        }

        if (NativeGallery.IsMediaPickerBusy())
            return;

        NativeGallery.GetImageFromGallery(
            path =>
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    onComplete?.Invoke(null);
                    return;
                }

                // IMPORTANT:
                // markTextureNonReadable MUST be false here because we need
                // to access the pixels and encode the processed avatar to PNG.
                Texture2D source = NativeGallery.LoadImageAtPath(
                    path,
                    MaxAvatarSize,
                    false,
                    false,
                    false
                );

                if (source == null)
                {
                    Debug.LogWarning("[EduAR] Could not load selected avatar: " + path);
                    onComplete?.Invoke(null);
                    return;
                }

                Texture2D square = null;

                try
                {
                    square = CropCenterToSquare(source);

                    if (square == null)
                    {
                        Debug.LogWarning("[EduAR] Could not prepare selected avatar.");
                        UnityEngine.Object.Destroy(source);
                        onComplete?.Invoke(null);
                        return;
                    }

                    string avatarDirectory = Path.Combine(
                        Application.persistentDataPath,
                        "EduAR_Avatars"
                    );

                    string avatarPath = Path.Combine(
                        avatarDirectory,
                        userId + ".png"
                    );

                    Directory.CreateDirectory(avatarDirectory);

                    // square is readable, so EncodeToPNG() is safe here.
                    byte[] png = square.EncodeToPNG();
                    File.WriteAllBytes(avatarPath, png);

                    UnityEngine.Object.Destroy(square);
                    UnityEngine.Object.Destroy(source);

                    Debug.Log("[EduAR] Avatar saved: " + avatarPath);
                    onComplete?.Invoke(avatarPath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[EduAR] Avatar save failed: " + ex.Message);

                    if (square != null)
                        UnityEngine.Object.Destroy(square);

                    if (source != null)
                        UnityEngine.Object.Destroy(source);

                    onComplete?.Invoke(null);
                }
            },
            "Choose Profile Photo",
            "image/*"
        );
#else
        onComplete?.Invoke(null);
#endif
    }

    private static Texture2D CropCenterToSquare(Texture2D source)
    {
        if (source == null || !source.isReadable)
            return null;

        int side = Mathf.Min(source.width, source.height);
        int offsetX = (source.width - side) / 2;
        int offsetY = (source.height - side) / 2;

        Color[] pixels = source.GetPixels(
            offsetX,
            offsetY,
            side,
            side
        );

        Texture2D square = new Texture2D(
            side,
            side,
            TextureFormat.RGBA32,
            false
        );

        square.SetPixels(pixels);
        square.Apply(false, false);

        return square;
    }

    public static void DeleteAvatar(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[EduAR] Avatar delete failed: " + ex.Message);
        }
    }
}
