using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace tetraveGame
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        ImageView imgGame, imgBlock;
        Bitmap gameBitmap, blockBitmap;
        TextView txt;
        int boardSize = 0;
        int gridSize = 0;
        TetraveGame game = new TetraveGame();
        int lastBoard = -1;
        int selectBlock = -1;

        float startx = 0;
        float starty = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //object p = Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
             SetContentView(Resource.Layout.activity_main);
            txt = (TextView)FindViewById(Resource.Id.textViewMsg);
            imgGame = (ImageView)FindViewById(Resource.Id.imageViewGame);
            imgBlock = (ImageView)FindViewById(Resource.Id.imageViewBlock);
            Button bt = (Button)FindViewById(Resource.Id.buttonGo);
            bt.Click+= (s, arg) => {
                PopupMenu menu = new PopupMenu(this, bt);
                menu.Inflate(Resource.Menu.gamemenu);
                menu.MenuItemClick += (s1, arg1) => {
                    switch(arg1.Item.ItemId)
                    {
                        case Resource.Id.action_new:
                            game.init();
                            break;
                        case Resource.Id.action_blank:
                            game.blankMatrix();
                            break;
                        case Resource.Id.action_retry:
                            game.retryGame();
                            break;
                        case Resource.Id.action_answer:
                            tryGame();
                            break;
                        case Resource.Id.action_3X3:
                            game.GAMESCALE = 3;
                            game.GAMEBLOCKS = 9;
                            game.makeGame();
                            break;
                        case Resource.Id.action_4X4:
                            game.GAMESCALE = 4;
                            game.GAMEBLOCKS = 16;
                            game.makeGame();
                            break;
                        case Resource.Id.action_5X5:
                            game.GAMESCALE = 5;
                            game.GAMEBLOCKS = 25;
                            game.makeGame();
                            break;
                        case Resource.Id.action_about:
                            Intent intent = new Intent(this, typeof(AboutActivity));
                            StartActivity(intent);
                            break;


                    }
                };

                menu.DismissEvent += (s2, arg2) => {
                };
                menu.Show();
            };
            EditText edit = (EditText)FindViewById(Resource.Id.editTextInput);
            edit.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    string input = edit.Text;
                    if (lastBoard != 1 || selectBlock == -1)
                    {
                        selectBlock = 1;
                        selectBlock = 0;
                    }
                    if (selectBlock >= game.GAMEBLOCKS )
                    {
                        Toast.MakeText(this, "已经全部输入完成！如果您想修改/输入指定的块，先点击屏幕下方对应的块即可...", ToastLength.Short).Show();
                        selectBlock = 0;
                        return;
                    }
                    int dir = 0;
                    while(input.Length >0)
                    {
                        char ch = input[0];
                        int v = input[0] - '0';
                        if (v >=0 && v <=9)
                        {
                            game.matrix[selectBlock * 4 + dir] = v;
                            game.chs[selectBlock] = selectBlock;
                            dir++;
                            if (dir >3)
                            {
                                selectBlock++;
                                if (selectBlock >= game.GAMEBLOCKS-1)
                                {
                                    break;
                                }
                                dir = 0;
                            }
                        }
                        input = input.Substring(1);
                    }
                    showChs();
                    edit.Text = "";
                    e.Handled = true;
                }
            };
            game.init();
            imgGame.Touch += onGameTuched;
            imgBlock.Touch += onBlockTuched;
        }

        private void tryGame()
        {
            bool isfull = true;
            for (int i = 0; i < game.GAMEBLOCKS; i++)
            {
                for (int j = 0;j<3;j++)
                {
                    if (game.matrix[i * 4 + j] < 0 || game.matrix[i * 4 + j] > 9)
                        isfull = false;
                }
            }
            if (isfull)
            {
                for(int i=0;i< game.GAMEBLOCKS;i++)
                {
                    game.chs[i] = i;
                    game.p[i] = -1;
                }
                game.qpl(game.chs, game.p, game.GAMESCALE * game.GAMESCALE);
                if (game.checkp(game.p))
                {
                    for(int i=0;i< game.GAMEBLOCKS;i++)
                    {
                        game.chs[i] = -1;
                    }
                    showGame();
                    
                    Toast.MakeText(this, "找到了...", ToastLength.Short).Show();
                }
            }
        }
        private void showChs()
        {
            for (int i = 0; i < game.GAMESCALE; i++)
                for (int j = 0; j < game.GAMESCALE; j++)
                {
                    drawGrid(imgBlock, blockBitmap, j, i, game.chs[i * game.GAMESCALE + j]);
                }
        }

        private void showGame()
        {
            Console.WriteLine("start showgame()...");
            for (int i = 0; i < game.GAMESCALE; i++)
                for (int j = 0; j < game.GAMESCALE; j++)
                {
                    int index = i * game.GAMESCALE + j;
                    if (game.p[index] == -1)
                    {
                        drawEmpty(imgGame, gameBitmap, j, i);
                    }
                    else
                    {
                        drawGrid(imgGame, gameBitmap, j, i, game.p[index]);
                    }
                    if (game.chs[index] == -1)
                    {
                        drawEmpty(imgBlock, blockBitmap, j, i);
                    }
                    else
                    {
                        drawGrid(imgBlock, blockBitmap, j, i, game.chs[index]);
                    }
                }
            Console.WriteLine("showGame() finished!");
        }

        private void onBlockTuched(object sender, View.TouchEventArgs e)
        {
            if(e.Event.Action != MotionEventActions.Up)
            {
                return;
            }
            float x = e.Event.GetX();
            float y = e.Event.GetY();
            int col = getGridNum(x);
            int row = getGridNum(y);
            if (col >= 0 && row >= 0)
            {
                int newCurrent = row * game.GAMESCALE + col;
                if (lastBoard==0 && selectBlock >=0 && game.chs[newCurrent]==-1)//如果有当前选中的放置块，并且本位置空，移回到本位置
                {
                    game.chs[newCurrent] = game.p[selectBlock]; //移内容，非指针
                    game.p[selectBlock] = -1;
                    drawEmpty(imgGame, gameBitmap, selectBlock % game.GAMESCALE, selectBlock / game.GAMESCALE); //抹除放置位
                    drawGrid(imgBlock, blockBitmap, col, row, game.chs[newCurrent]); //抹除放置位
                    selectBlock = -1;
                    lastBoard = -1;
                    return;
                }
                else if (lastBoard ==1 && selectBlock>=0 && game.chs[newCurrent] == -1) //本区移动
                {
                    game.chs[newCurrent] = game.chs[selectBlock];
                    game.chs[selectBlock] = -1;
                    drawEmpty(imgBlock, blockBitmap, selectBlock % game.GAMESCALE, selectBlock / game.GAMESCALE); //抹除放置位
                    drawGrid(imgBlock, blockBitmap, col, row, game.chs[newCurrent]); //抹除放置位
                    selectBlock = -1;
                    lastBoard = -1;
                }
                else if (game.chs[newCurrent] != -1) //否则，选中本位置为预备块
                {
                    selectBlock = newCurrent;
                    lastBoard = 1;
                    drawFrame(imgBlock, blockBitmap, col, row, false);
                }
            }
        }
        private void onGameTuched(object sender, View.TouchEventArgs e)
        {
            switch(e.Event.Action)
            {
                case MotionEventActions.Down:
                    startx = e.Event.GetX();
                    starty = e.Event.GetY();
                    break;
                case MotionEventActions.Up:
                    int action = getTouchAction(startx,  starty, e.Event.GetX(), e.Event.GetY());
                    int col = getGridNum(startx);
                    int row = getGridNum(starty);
                    if (action ==0)
                    {
                        int newCurrent = row * game.GAMESCALE + col;
                        if (col >= 0 && row >= 0)
                        {
                            if (lastBoard == 1&& selectBlock>=0 && game.p[newCurrent] == -1)//如果备选有选中，并且本位空，移动过来
                            {
                                game.p[newCurrent] = game.chs[selectBlock];
                                game.chs[selectBlock] = -1;
                                drawGrid(imgGame, gameBitmap, col, row, game.p[newCurrent]);
                                drawEmpty(imgBlock, blockBitmap, selectBlock % game.GAMESCALE, selectBlock / game.GAMESCALE);   //消除备选位
                                lastBoard = -1;
                                lastBoard = -1;
                                if (game.checkp(game.p))
                                {
                                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                                    builder.SetTitle("恭喜");
                                    builder.SetMessage("你成功了！");
                                    builder.SetIcon(Resource.Drawable.notification_tile_bg);
                                    builder.SetCancelable(true);
                                    builder.Show();
                                }
                                return;
                            }
                            else if (lastBoard == 0 && game.p[selectBlock] >= 0 && game.p[newCurrent] == -1) //否则，上次选择就是本区，作为本区内移动
                            {
                                game.p[newCurrent] = game.p[selectBlock];
                                game.p[selectBlock] = -1;
                                drawGrid(imgGame, gameBitmap, col, row, game.p[newCurrent]);
                                drawEmpty(imgGame, gameBitmap, selectBlock % game.GAMESCALE, selectBlock / game.GAMESCALE);   //消除备选位
                                lastBoard = -1;
                                lastBoard = -1;
                                return;
                            }
                            else //更新点击区和点击块
                            {
                                lastBoard = 0;
                                selectBlock = newCurrent;
                                drawFrame(imgGame, gameBitmap, col, row,false);
                            }
                        }
                    }
                    else
                    {
                        if (game.JumpLine(action))
                            showGame();
                    }
                    break;
            }

        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                Console.WriteLine("Enter windowsFoucsChanged...");
                LinearLayout layout = (LinearLayout)FindViewById(Resource.Id.linearLayoutGame);
                int layoutWidth = layout.Width;
                int layoutHeight = layout.Height;
                boardSize = (layoutHeight-5) >= 2 * layoutWidth ? layoutWidth : (layoutHeight-5) / 2;
                gridSize = boardSize/ game.GAMESCALE;
                gameBitmap = Bitmap.CreateBitmap(boardSize, boardSize, Bitmap.Config.Argb8888);
                blockBitmap = Bitmap.CreateBitmap(boardSize, boardSize, Bitmap.Config.Argb8888);
                drawBoard(imgGame, gameBitmap, Color.Red);
                drawBoard(imgBlock, blockBitmap, Color.Purple);
                showGame();
            }
            base.OnWindowFocusChanged(hasFocus);
            Console.WriteLine("windowsFoucsChanged done!");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        void drawBoard(ImageView img, Bitmap bitmap,Color color)
        {
            Canvas canvs = new Canvas(bitmap);
            Paint paint = new Paint();
            paint.Color = color;
            paint.StrokeWidth = 3;
            paint.SetStyle(Paint.Style.Stroke);
            canvs.DrawRect(2, 2, boardSize-2, boardSize-2,paint);
            
            for(int i=1;i< game.GAMESCALE;i++)
            {
                int line = i * gridSize;
                canvs.DrawLine(line, 0, line, boardSize, paint);//画横线
                canvs.DrawLine(0, line, boardSize, line, paint);//画竖线
            }
            img.SetImageBitmap(bitmap);
        }
        void drawGrid(ImageView img, Bitmap bm,int col, int row, int index)
        {
            int fl = gridSize - TetraveGame.GridFree;
            int hl = fl / 2;
            int ql = hl / 2;
            int x = col * gridSize + TetraveGame.GridFree;
            int y = row * gridSize + TetraveGame.GridFree;

            int value = game.matrix[index * 4];
            Canvas canvas = new Canvas(bm);
            Paint paint = new Paint();
            paint.Color = game.getColor(game.NUMBAKE[value]);
            paint.SetStyle(Paint.Style.Fill);
            paint.TextSize = ql;
            paint.TextAlign = Paint.Align.Center;
            Path path = new Path();
            //up
            path.MoveTo(x, y);
            path.LineTo(x+fl, y);
            path.LineTo(x + hl, y + hl);
            path.LineTo(x, y);
            canvas.DrawPath(path, paint);
            paint.Color = game.getColor(game.NUMCOLOR[value]);
            canvas.DrawText(value.ToString(), x + hl, y + ql, paint);
            path.Reset();
            //left
            value = game.matrix[index * 4 + 1];
            paint.Color = game.getColor(game.NUMBAKE[value]);
            path.MoveTo(x, y);
            path.LineTo(x, y + fl);
            path.LineTo(x + hl, y + hl);
            path.LineTo(x, y);
            canvas.DrawPath(path, paint);
            paint.Color = game.getColor(game.NUMCOLOR[value]);
            canvas.DrawText(value.ToString(), x + ql, y + hl, paint);
            path.Reset();
            //right
            value = game.matrix[index * 4 + 2];
            paint.Color = game.getColor(game.NUMBAKE[value]);
            path.MoveTo(x+fl, y);
            path.LineTo(x+fl, y + fl);
            path.LineTo(x + hl, y + hl);
            path.LineTo(x+fl, y);
            canvas.DrawPath(path, paint);
            paint.Color = game.getColor(game.NUMCOLOR[value]);
            canvas.DrawText(value.ToString(), x + 3*ql, y + hl, paint);
            path.Reset();

            //down
            value = game.matrix[index * 4 + 3];
            paint.Color = game.getColor(game.NUMBAKE[value]);
            path.MoveTo(x, y + fl);
            path.LineTo(x + fl, y + fl);
            path.LineTo(x + hl, y + hl);
            path.LineTo(x, y + fl);
            canvas.DrawPath(path, paint);
            paint.Color = game.getColor(game.NUMCOLOR[value]);
            canvas.DrawText(value.ToString(), x + hl, y + 3*ql, paint);
            img.SetImageBitmap(bm);
        }
        void drawFrame(ImageView img, Bitmap bm, int col, int row,bool earse)
        {
            Canvas canvs = new Canvas(bm);
            int x = col * gridSize + 2;
            int y = row * gridSize + 2;
            Paint paint = new Paint();
            paint.StrokeWidth = 3;
            if (earse)
            {
                paint.Color = Color.White;
            }
            else
            {
                paint.Color = Color.DarkGreen;
            }
            paint.SetStyle(Paint.Style.Stroke);
            canvs.DrawRect(x, y, x + gridSize - 4, y + gridSize - 4, paint);
            img.SetImageBitmap(bm);
        }
        void drawEmpty(ImageView img, Bitmap bm,int col, int row)
        {
            Canvas canvs = new Canvas(bm);
            int x = col * gridSize;
            int y = row * gridSize;
            Paint paint = new Paint();
            paint.StrokeWidth = 1;
            paint.Color = Color.White;
            paint.SetStyle(Paint.Style.Fill);
            canvs.DrawRect(x, y, x + gridSize , y + gridSize , paint);
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.DarkCyan;
            canvs.DrawRect(x, y, x + gridSize, y + gridSize, paint);
            img.SetImageBitmap(bm);
        }
        void test()
        {
            Bitmap bitmap = Bitmap.CreateBitmap(600, 600, Bitmap.Config.Argb8888);
            Canvas canvs = new Canvas(bitmap);
            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.Fill);
            paint.Color = Color.Red;
            canvs.DrawRect(0, 0, 300, 600, paint);
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.Blue;
            paint.StrokeWidth = 3;
            canvs.DrawRect(300, 0, 600, 600, paint);
            imgGame.SetImageBitmap(bitmap);
            Bitmap bm = Bitmap.CreateBitmap(800, 800, Bitmap.Config.Argb8888);
            Canvas bc = new Canvas(bm);
            paint.Color = Color.DeepSkyBlue;
            paint.SetStyle(Paint.Style.Fill);
            Path path = new Path();
            path.MoveTo(10, 10);
            path.LineTo(790, 10);
            path.LineTo(400, 790);
            path.LineTo(10, 10);
            bc.DrawPath(path, paint);
            paint.TextAlign = Paint.Align.Center;
            paint.TextSize = 50;
            paint.Color = Color.White;
            bc.DrawText("5", 400, 400, paint);
            imgBlock.SetImageBitmap(bm);
        }
        int getGridNum(float xy)
        {
            int num = (int)Math.Truncate(xy/gridSize);
            if (xy > num * gridSize + TetraveGame.GridFree && xy < (num + 1) * gridSize - TetraveGame.GridFree)
                return num;
            else
                return -1;
        }
        int getTouchAction(float x1,float y1,float x2,float y2)
        {

            float x = x2 - x1;
            float ax = Math.Abs(x);
            float y = y2 - y1;
            float ay = Math.Abs(y);
            if (ax >400 && ay > 400)
            {
                if (Math.Abs(ax-ay) < (ax<ay?ax:ay))
                {
                    if (x > 0 && y < 0)
                        return 2;
                }
                else if (x>0 && y>0)
                {
                    return 4;
                }
                else if(x<0 && y>0)
                {
                    return 6;
                }
                return 8;
            }
            else if (ax>400 || ay >400)
            {
                if (ax >ay)
                {
                    if (x > 0)
                    {
                        return 3;
                    }
                    else
                    {
                        return 7;
                    }
                }
                else
                {
                    if (y>0)
                    {
                        return 5;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
    }
    
}