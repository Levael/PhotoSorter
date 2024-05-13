using B83.Image.BMP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

#nullable enable

public class ImagesHandler : MonoBehaviour
{
    private Main main;
    private ConfigHandler configHandler;
    private UiHandler uiHandler;

    private VisualElement imagesContainer;
    private Image backgroundImage;
    private Image mainImage;

    public Queue<string> imagesBlob;
    private int imagesBlobSize;
    public string folderNameForCorruptedFiles;


    void Awake()
    {
        main = GetComponent<Main>();
        configHandler = GetComponent<ConfigHandler>();
        uiHandler = GetComponent<UiHandler>();


        imagesBlob = new();
        imagesBlobSize = 200;
        folderNameForCorruptedFiles = "CorruptedFiles";
    }

    void Start()
    {
        imagesContainer = uiHandler.imagesContainer;
        mainImage = uiHandler.mainImage;
        backgroundImage = uiHandler.backgroundImage;
    }



    // PUBLIC
    public void ClearImageAndItsData()
    {
        mainImage.image = null;
        backgroundImage.image = null;

        mainImage.RemoveFromClassList("image-border-left-right");
        mainImage.RemoveFromClassList("image-border-up-down");

        uiHandler.FillImageInfo(
            name: "",
            path: "",
            size: "",
            extension: ""
        );
    }

    public void ShowNextImage()
    {
        if (imagesBlob.Count == 0)
        {
            LoadBunchOfImages(configHandler.fields.sourceFolderFullName);
        }

        if (imagesBlob.Count == 0)
        {
            ClearImageAndItsData();
            main.currentFileOriginFullPath = null;
            main.currentFileDestinationFullPath = null;
            uiHandler.PrintWarning_OutOfFiles();
            return;
        }

        ShowImage(imagesBlob.Dequeue());
    }

    public void ShowPreviousImage()
    {
        ShowImage(main.lastFileOriginFullPath); // moved before showing
    }


    // PRIVATE
    private void LoadBunchOfImages(string folderPath)
    {
        if (String.IsNullOrEmpty(folderPath)) return;

        string[] imageExtensions = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" };    // "*.svg", "*.gif" 
        imagesBlob = new Queue<string>(imageExtensions.SelectMany(ext => Directory.GetFiles(folderPath, ext)).Take(imagesBlobSize));
    }

    private void ShowImage(string fileFullPath)
    {
        try
        {
            DisplayImage(fileFullPath);
        }
        catch (Exception ex_1)
        {
            try
            {
                var destinationForCorruptedFile = Path.Combine(configHandler.fields.sourceFolderFullName, folderNameForCorruptedFiles, Path.GetFileName(fileFullPath));
                FilesHandler.MoveFile(fileFullPath, destinationForCorruptedFile);
                Debug.LogWarning($"File moved to corrupted folder. Exception: {ex_1}");
            }
            catch (Exception ex_2)
            {
                Debug.LogError($"Ultra total error while moving file to corrupted folder. Exception: {ex_2}");
                return;
            }


            // blink with red
            ShowNextImage();    // it's a recursion, but should be ok... I hope // this line is sooo muddy..... // TODO
            return;
        }

        main.currentFileOriginFullPath = fileFullPath;

        uiHandler.FillImageInfo(
            name: Path.GetFileNameWithoutExtension(fileFullPath),
            path: Path.GetDirectoryName(fileFullPath),
            size: $"{(new FileInfo(fileFullPath).Length / 1024.0 / 1024.0):F2} MB",
            extension: Path.GetExtension(fileFullPath).Substring(1)     // deletes a dot
        );

        // clear any warning/error message because all right
        uiHandler.ClearConsole();
    }

    private void DisplayImage(string fileFullPath)
    {
        var texture = LoadTextureFromFile(fileFullPath);

        DisplayMainImage(texture);
        DisplayBackgroundImage(texture);
    }

    private void DisplayMainImage(Texture2D texture)
    {
        // eliminates small inconsistencies by several pixels when setting the dimensions
        texture.wrapMode = TextureWrapMode.Clamp;

        var containerWidth = imagesContainer.resolvedStyle.width;
        var containerHeight = imagesContainer.resolvedStyle.height;

        float imageAspectRatio = (float)texture.width / texture.height;
        float containerAspectRatio = containerWidth / containerHeight;

        float targetWidth, targetHeight;
        if (imageAspectRatio > containerAspectRatio)
        {
            // Horizontal Image

            targetWidth = containerWidth;
            targetHeight = targetWidth / imageAspectRatio + 20; // 10px from each border

            mainImage.RemoveFromClassList("image-border-left-right");
            mainImage.AddToClassList("image-border-up-down");
        }
        else
        {
            // Vertical Image

            targetHeight = containerHeight;
            targetWidth = targetHeight * imageAspectRatio + 20; // 10px from each border

            mainImage.RemoveFromClassList("image-border-up-down");
            mainImage.AddToClassList("image-border-left-right");
        }

        mainImage.style.width = targetWidth;
        mainImage.style.height = targetHeight;

        mainImage.image = texture;
    }

    private void DisplayBackgroundImage(Texture2D texture)
    {
        backgroundImage.image = BlurImageViaResample(texture, 200);   // such a big number is on purpose 
    }
    

    private Texture2D BlurImageViaResample(Texture2D original, float downscaleFactor)
    {
        int downscaledWidth = Mathf.Max(1, (int)(original.width / downscaleFactor));
        int downscaledHeight = Mathf.Max(1, (int)(original.height / downscaleFactor));

        RenderTexture rt = RenderTexture.GetTemporary(downscaledWidth, downscaledHeight);
        rt.filterMode = FilterMode.Bilinear;

        Graphics.Blit(original, rt);

        RenderTexture rtUpscaled = RenderTexture.GetTemporary(original.width, original.height);
        rtUpscaled.filterMode = FilterMode.Bilinear;

        Graphics.Blit(rt, rtUpscaled);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rtUpscaled;
        Texture2D upscaledTexture = new Texture2D(original.width, original.height);
        upscaledTexture.ReadPixels(new Rect(0, 0, rtUpscaled.width, rtUpscaled.height), 0, 0);
        upscaledTexture.Apply();
        RenderTexture.active = previous;

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(rtUpscaled);

        return upscaledTexture;
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        if (Path.GetExtension(filePath) == ".bmp")
        {
            Texture2D texture = null;
            BMPLoader bmpLoader = new BMPLoader();
            bmpLoader.ForceAlphaReadWhenPossible = true;
            BMPImage bmpImg = bmpLoader.LoadBMP(filePath);
            texture = bmpImg.ToTexture2D();
            return texture;
        }
        else
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }
    }
}