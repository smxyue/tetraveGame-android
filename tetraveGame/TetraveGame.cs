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
        public const int GAMESCALE = 4;
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
        public int[] matrix = { 1,6,8,7,2,8,7,5,4,7,5,3,3,5,4,7,4,4,1,4,7,0,0,3,5,0,3,0,3,3,4,5,7,4,8,7,4,8,1,2,3,4,8,2,0,8,2,6,5,2,2,7,7,2,8,7,2,8,6,0,2,2,3,3,6,3,9,6,7,9,0,6,7,0,8,3,0,8,6,6,1,1,5,4,7,5,8,7,6,8,7,1,7,7,4,1,3,4,1,2};
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
            //randomMatrix();
            makeGame();
            for(int i=0;i<GAMEBLOCKS;i++)
            {
                p[i] = -1;
            }
        }
        void CloneChs(int[] old, int[] clone, int index, int size) //旧序列  新序列  要移除的元素索引  要克隆的序列长度。
        {
            //去除某个特定的元素后形成新序列，空白填充-1
            for (int i = 0; i < index; i++)
            {
                clone[i] = old[i];
            }
            for (int i = index; i < size - 1; i++)
            {
                clone[i] = old[i + 1];
            }
            for (int i = size; i < GAMESCALE * GAMESCALE; i++)
                clone[i] = -1;
        }
        void CloneP(int[] oldp, int[] newp)
        {
            //复制已经排列好的位置的值，并把
            for (int i = 0; i < GAMESCALE * GAMESCALE; i++)
            {
                newp[i] = oldp[i];
            }
        }
        bool preCheck(int[] pp, int len) //排列数组   长度
        {
            for (int i = 0; i < len; i++)
            {
                if (!checkItem(pp, i, pp[i]))
                {
                    return false;
                }
            }
            return true;
        }
        bool checkItem(int[] pp, int grid, int index) //格位  值位
        {
            if (index == -1)
                return true;
            int tindex;
            int row = grid / GAMESCALE;
            int col = grid % GAMESCALE;
            //上
            if (row > 0)
            {
                tindex = (row - 1) * GAMESCALE + col; //行少列同
                tindex = pp[tindex];//该位实际的块是第几个。
                if (tindex != -1) //没有填
                {
                    if (matrix[index * 4] != matrix[tindex * 4 + 3])
                    {
                        return false;
                    }
                }
            }
            //右
            if (col < (GAMESCALE - 1))
            {
                tindex = row * GAMESCALE + col + 1;
                tindex = pp[tindex];
                if (tindex != -1)
                {
                    if (matrix[index * 4 + 2] != matrix[tindex * 4 + 1])
                    {
                        return false;
                    }
                }
            }
            //下
            if (row < (GAMESCALE - 1))
            {
                tindex = (row + 1) * GAMESCALE + col;
                tindex = pp[tindex];
                if (tindex != -1)
                {
                    if (matrix[index * 4 + 3] != matrix[tindex * 4])
                    {
                        return false;
                    }
                }
            }
            //左
            if (col > 0)
            {
                tindex = row * GAMESCALE + col - 1;
                tindex = pp[tindex];
                if (tindex != -1)
                {
                    if (matrix[index * 4 + 1] != matrix[tindex * 4 + 2])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool checkp(int[] pp)
        {
            int tindex;//相邻位
            for (int i = 0; i < GAMESCALE * GAMESCALE; i++)//检查所有9个位置是否都正确
            {
                int index = pp[i]; //当排位为i的实际是matrix里的哪个
                if (!checkItem(pp, i, index))
                    return false;
            }
            return true;
        }
        public void qpl(int[] chs, int[] pp, int size) //待选数组， 排序数组，排列长度
        {
            //从最低位开始，依次填入待选数组里的所有元素，然后把这个顺序号去掉，用剩下的可选位排其他的
            if (size == 1)
            {
                pp[GAMESCALE * GAMESCALE - 1] = chs[0];
                if (checkp(pp))
                {
                    CloneP(pp, p);
                }
             }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    int[] newP = new int[GAMESCALE * GAMESCALE];
                    for (int j = 0; j < GAMESCALE * GAMESCALE; j++)
                    {
                        newP[j] = pp[j];
                    }
                    newP[GAMESCALE * GAMESCALE - size] = chs[i];
                    int[] newChs = new int[GAMESCALE * GAMESCALE];
                    for (int j = 0; j < i; j++)
                    {
                        newChs[j] = chs[j];
                    }
                    for (int j = i; j < size - 1; j++)
                    {
                        newChs[j] = chs[j + 1];
                    }
                    for (int j = size; j < GAMESCALE * GAMESCALE; j++)
                        newChs[j] = -1;
                    int newSize = size - 1;
                    if (preCheck(newP, GAMESCALE * GAMESCALE - size))//如果前置序列已经不对，后续的所有序列都不需要试探了
                    {
                        qpl(newChs, newP, newSize);
                    }
                }
            }
        }
        public int neighborCloset(int closet)
        {
            int index = (int)Math.Truncate(closet / 4.0);
            int dir = closet % 4;
            int row = index / GAMESCALE;
            int col = index % GAMESCALE;
            switch(dir)
            {
                case 0:
                    if (row > 0)
                        return closet - 4*GAMESCALE +3;  //返回上一行同列的3位
                    else
                        return -1;
                case 1:
                    if (col > 0)
                        return closet - 3;
                    else
                        return -1;
                case 2:
                    if (col < GAMESCALE - 1)
                        return closet - 3;
                    else
                        return -1;
                case 3:
                    if (row < GAMESCALE - 1)
                        return closet + 4 * GAMESCALE - 3;
                    else
                        return -1;
                default:
                    return -1;
                            
            }
        }
        public void makeGame()
        {
            for (int i = 0; i < GAMEBLOCKS * 4; i++)
            {
                matrix[i] = -1;
            }
            Random rand = new Random();
            for (int i = 0; i < GAMEBLOCKS * 4; i++)
            {
                
                if (matrix[i] == -1)
                {
                    int nc = neighborCloset(i);
                    int v = rand.Next(10);
                    if (nc != -1)
                    {
                        if (matrix[nc] != -1)
                        {
                            matrix[i] = matrix[nc];
                        }
                        else
                        {
                            
                            matrix[i] = v;
                            matrix[nc] = v;
                        }
                    }
                    else
                    {
                        matrix[i] = v;
                    }
                }
            }
            for(int i=0;i<GAMEBLOCKS;i++)
            {
                chs[i] = -1;
            }
            for (int i = 0; i < GAMEBLOCKS; i++)
            {
                int room = rand.Next(GAMEBLOCKS);
                while (chs.Contains(room))
                {
                    room = rand.Next(GAMEBLOCKS);
                }
                chs[i] = room;
            }
        }
    }
}