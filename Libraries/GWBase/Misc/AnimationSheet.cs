using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using GWBase;
using UnityEngine;

namespace GWBase
{
    [Serializable]
    public class AnimationSheet
    {
        [Serializable]
        [XmlRoot("AnimationDef")]
        public struct AnimationSheetInfo
        {
            [XmlAttribute("Name")]
            public string typeName;
            [XmlAttribute("SheetName")]
            public string sheetName;
            [XmlElement("frameCount")]
            public int frameCount;
            [XmlElement("framePerSecond")]
            public int framePerSecond;
            [XmlElement("doesLoop")]
            public string doesLoop;
        }

        public AnimationSheetInfo info;

        private Texture2D sheet;       
        private List<Texture2D> frames;
        public List<Sprite> calculatedFrames = new List<Sprite>();

        public void CalculateSheet()
        {
            for (int i = 0; i < info.frameCount; i++)
            {
                var targetAnimFrame = frames[i];

                Sprite newSprite = Sprite.Create(targetAnimFrame, new Rect(0, 0, targetAnimFrame.width, targetAnimFrame.height), new Vector2(0.5f, 0.5f));
                calculatedFrames.Add(newSprite);
            }
        }

        public void Initiate(Texture2D sheet, int size)
        {
            this.sheet = sheet;
            Split(sheet, size, 1, out frames);
            CalculateSheet();
        }

        public void Split(Texture2D imageToCrop, int columns, int rows, out List<Texture2D> splittedImages)
        {
            splittedImages = new List<Texture2D>();
            int pieceWidth = imageToCrop.width / columns;
            int pieceHeight = imageToCrop.height / rows;

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Texture2D piece = new Texture2D(pieceWidth, pieceHeight);
                    UnityEngine.Color[] pixels = imageToCrop.GetPixels(i * pieceWidth, j * pieceHeight, pieceWidth, pieceHeight);
                    piece.SetPixels(pixels);
                    piece.Apply();
                    splittedImages.Add(piece);
                }
            }
        }

        public void Save(string path)
        {
            {
                if (frames != null && frames.Count > 0)
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        byte[] bytes = frames[i].EncodeToPNG();
                        File.WriteAllBytes(Path.Combine(path, $"{info.typeName}_{i}.png"), bytes);
                    }
                }
            }
        }
    }
}