using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tetraveGame
{
    class TetraveGame
    {
        public const int GAMESCALE = 5;
        public const int GAMEBLOCKS = GAMESCALE * GAMESCALE;
        public const int GridFree = 5;
        public int[] NUMBAKE = { 0x3e3947, 0xc11d29, 0xffa449, 0xf7d42e, 0x59e58b, 0xb6845b, 0x9ac2f2, 0x1c61b6, 0xc162cc, 0xf7f6f5 };
        public int[] NUMCOLOR = { 0xf7f6f5, 0xf7f6f5, 0x3e3947, 0x3e3947, 0x3e3947, 0xf7f6f5, 0x3e3947, 0xf7f6f5, 0xf7f6f5, 0x3e3947, };
        struct BackPoint
        {
            char room; //位置
            char dir;
            char neighbor; //适配块
        };
        public int[] matrix = { '1', '6', '8', '7', '2', '8', '7', '5', '4', '7', '5', '3', '3', '5', '4', '7', '4', '4', '1', '4', '7', '0', '0', '3', '5', '0', '3', '0', '3', '3', '4', '5', '7', '4', '8', '7', '4', '8', '1', '2', '3', '4', '8', '2', '0', '8', '2', '6', '5', '2', '2', '7', '7', '2', '8', '7', '2', '8', '6', '0', '2', '2', '3', '3', '6', '3', '9', '6', '7', '9', '0', '6', '7', '0', '8', '3', '0', '8', '6', '6', '1', '1', '5', '4', '7', '5', '8', '7', '6', '8', '7', '1', '7', '7', '4', '1', '3', '4', '1', '2' };
        public int[] p = new int[GAMEBLOCKS];
        public int[] chs = new int[GAMEBLOCKS];
        public int tryIndex = 0;
        public int[] tryOrder = new int[GAMEBLOCKS]; //用过的块号
        List<BackPoint> backList = new List<BackPoint>();
        int firstRoom = 0;
        void initTryOrder()
        {
            for (int i = 0; i < GAMEBLOCKS; i++)
            {
                tryOrder[i] = -1;
                p[i] = -1;
            }
            backList.Clear();
            tryIndex = 0;
        }
        public void randomMatrix()
        {
            Random rand = new Random();
            for (int i = 0; i < GAMEBLOCKS * 4; i++)
            {
                matrix[i] = (char)rand.Next(10);
            }
        }
        public String getMatrixChar(int index, int site)
        {
            return matrix[index * 4 + site].ToString();
        }
        public Color getColor(int c)
        {
            byte r = (byte)((c & 0xff0000) >> 16);
            byte g = (byte)((c & 0x00ff00) >> 8);
            byte b = (byte)(c & 0x0000ff);
            return new Color(r, g, b);
        }
        public void init()
        {
            randomMatrix();
            for(int i=0;i<GAMEBLOCKS;i++)
            {
                p[i] = -1;
                chs[i] = i;
            }
        }
    }
}