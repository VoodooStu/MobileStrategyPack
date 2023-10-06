using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Voodoo.Sauce.Core;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.SplashScreen
{
   public class SplashScreenPrebuild : IPreprocessBuildWithReport
   {
      private const int WIDTH = 1024;
      private const int HEIGHT = 1024;
      private const int MARGIN = 192;

      private const string VOODOO_LOGO_PATH = "Assets/VoodooSauce/SplashScreen/Editor/VoodooLogoSquare.png";
      private const string SPLASH_SCREEN_PATH = "Assets/Generated/SplashScreen/SplashScreen.png";

      public int callbackOrder => 1;

      public void OnPreprocessBuild(BuildReport report)
      {
         GenerateSplashScreen();
      }
      
      [MenuItem("VoodooSauce/Generate splash screen")]
      private static void GenerateSplashScreen()
      {
         VoodooSettings settings = VoodooSettings.Load();
         if (settings.DisableVSManagedSplashScreen) {
            Debug.Log("[VoodooSauce] splash screen generation disabled");
            return;
         }
         
         try {
            Sprite splashScreenSprite = GenerateSplashScreen(settings.StudioLogoForSplashScreen);
            PlayerSettings.SplashScreen.logos = new[] {PlayerSettings.SplashScreenLogo.Create(3f, splashScreenSprite)};
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.overlayOpacity = 0;
            Debug.Log("[VoodooSauce] splash screen generated");
         } catch (Exception e) {
            Debug.LogError($"[VoodooSauce] can not generate the splash screen: {e}");
         }
      }
      
      private static Sprite GenerateSplashScreen(Texture2D studioTexture)
      {
         var colorsQueue = new Queue<Color32[]>();
         
         var voodooSprite = (Sprite) AssetDatabase.LoadAssetAtPath(VOODOO_LOGO_PATH, typeof(Sprite));
         Color32[] voodooColors = ConvertAndResizeTextureToColorArray(voodooSprite.texture,WIDTH, voodooSprite.texture.height);

         if (studioTexture != null) {
            //Change import settings of studio logo so that pixels can be read
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(studioTexture));
            textureImporter.isReadable = true;
            textureImporter.textureType = TextureImporterType.Sprite;
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
            int studioLogoHeight = HEIGHT -MARGIN -voodooSprite.texture.height;
            Color32[] studioColors = ConvertAndResizeTextureToColorArray(studioTexture,WIDTH, studioLogoHeight);
            colorsQueue.Enqueue(studioColors);
            
            var margin = new Color32[WIDTH * MARGIN];
            colorsQueue.Enqueue(margin);
         }
         
         colorsQueue.Enqueue(voodooColors);
         
         Texture2D splashScreen = CreateTexture(colorsQueue,WIDTH);
         Sprite sprite = SaveTextureToSprite(splashScreen, SPLASH_SCREEN_PATH);
         return sprite;
      }

      private static Texture2D CreateTexture(Queue<Color32[]> colorsQueue, int width)
      {
         int height = 0;
         int currentHeight = 0;
         foreach (var colors in colorsQueue) {
            height += colors.Length / width;
         }

         var texture = new Texture2D(width, height);
         while (colorsQueue.Count > 0) {
            Color32[] colors = colorsQueue.Dequeue();
            int blockHeight = colors.Length / width;
            texture.SetPixels32(0, currentHeight, width, blockHeight, colors);
            currentHeight += blockHeight;
         }
         
         texture.Apply();
         return texture;
      }

      private static Color32[] ConvertAndResizeTextureToColorArray(Texture2D texture, int width, int height, int sideMargin = 0, int topMargin = 0, int bottomMargin = 0)
      {

         if (texture.width == width && texture.height == height) return texture.GetPixels32();
         //Calculate logo size limits
         int logoMaxWidth = width - sideMargin * 2;
         int logoMaxHeight = height - topMargin - bottomMargin;
         float desiredAspectRatio = (float) logoMaxWidth / logoMaxHeight;
         float givenLogoAspectRatio = (float) (texture.width) / (texture.height);
         int logoActualWidth = givenLogoAspectRatio >= desiredAspectRatio
            ? logoMaxWidth
            : (int) (logoMaxHeight * givenLogoAspectRatio);
         int logoActualHeight = givenLogoAspectRatio >= desiredAspectRatio
            ? (int) (logoMaxWidth / givenLogoAspectRatio)
            : logoMaxHeight;

         //Resize logo
         var resizedLogo = new Color32[logoActualWidth * logoActualHeight];
         float xDelta = 1.0f / logoActualWidth;
         float yDelta = 1.0f / logoActualHeight;
         for (var i = 0; i < resizedLogo.Length; i++) {
            resizedLogo[i] = texture.GetPixelBilinear(xDelta * (i % logoActualWidth), yDelta * (i / logoActualWidth));
         }

         //Pad resized logo to fit splash screen logo
         int paddedLeftMargin = sideMargin + (logoMaxWidth - logoActualWidth) / 2;
         int paddedBottomMargin = bottomMargin + (logoMaxHeight - logoActualHeight) / 2;
         var finalLogo = new Color32[width * height];
         for (var i = 0; i < finalLogo.Length; i++) {
            int x = i % width;
            int y = i / width;
            if (x < paddedLeftMargin || x >= paddedLeftMargin + logoActualWidth || y < paddedBottomMargin
                || y >= paddedBottomMargin + logoActualHeight)
               finalLogo[i] = Color.clear;
            else
               finalLogo[i] = resizedLogo[(y - paddedBottomMargin) * logoActualWidth + (x - paddedLeftMargin)];
         }

         return finalLogo;
      }
      
      private static Sprite SaveTextureToSprite(Texture2D texture, string path)
      {
         DirectoryInfo directoryInfo = new FileInfo(path).Directory;
         if (directoryInfo != null) directoryInfo.Create();
         File.WriteAllBytes(path, texture.EncodeToPNG());
         AssetDatabase.Refresh();
      
         AssetImporter importer = AssetImporter.GetAtPath(path);
         if (importer is TextureImporter textImporter) {
            textImporter.textureType = TextureImporterType.Sprite;
            textImporter.spritePivot = new Vector2(0.5f, 0.5f);
            textImporter.spritePixelsPerUnit = 100f;
            EditorUtility.SetDirty(textImporter);
            textImporter.SaveAndReimport();
         }
         AssetDatabase.Refresh();   
         
         return AssetDatabase.LoadAssetAtPath<Sprite>(path);
      }
   }
}