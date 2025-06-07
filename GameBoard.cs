// ============================
// File: GameBoard.cs (refatorado)
// ============================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClientKingMe
{
    internal class GameBoard
    {
        private readonly int[] floorPositionMap = new int[6];
        private readonly Dictionary<char, Image> characterImages = new Dictionary<char, Image>();
        private readonly PictureBox boardView;
        private readonly List<PictureBox> charactersOnBoard = new List<PictureBox>();

        public GameBoard(PictureBox boardView)
        {
            this.boardView = boardView;
            LoadCharacterImages();
        }

        private void LoadCharacterImages()
        {
            try
            {
                string basePath = ApplicationConstants.ImagesFolderPath;
                foreach (var def in ApplicationConstants.CharacterDefinitions)
                {
                    characterImages[def.Code] = Image.FromFile($"{basePath}/{def.Code}.png");
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError($"Erro ao carregar imagens: {ex.Message}");
            }
        }

        public bool PlaceCharacter(char characterCode, int floor)
        {
            if (floor < 0 || floor >= floorPositionMap.Length || !characterImages.ContainsKey(characterCode))
                return false;

            PictureBox characterPic = new PictureBox()
            {
                Image = characterImages[characterCode],
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            int floorWidth = boardView.Width - 100;
            int floorHeight = (boardView.Height / 6) - 20;
            int posX = 60 + (floorPositionMap[floor] * (floorWidth / 4));
            int posY = boardView.Height - ((floor + 1) * floorHeight);

            characterPic.Size = new Size(floorWidth / 5, floorHeight / 2);
            characterPic.Location = new Point(posX, posY);

            boardView.Controls.Add(characterPic);
            characterPic.BringToFront();

            floorPositionMap[floor]++;
            charactersOnBoard.Add(characterPic);

            return true;
        }

        public void ClearBoard()
        {
            foreach (var pic in charactersOnBoard)
            {
                boardView.Controls.Remove(pic);
                pic.Dispose();
            }

            charactersOnBoard.Clear();

            for (int i = 0; i < 6; i++)
            {
                floorPositionMap[i] = 0;
            }
        }

        public void ProcessBoardUpdate(string serverResponse)
        {
            ClearBoard();

            string[] lines = serverResponse.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int floor))
                {
                    char characterCode = parts[1][0];
                    PlaceCharacter(characterCode, floor);
                }
            }
        }
    }
}
