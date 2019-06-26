﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using WindowScrape;
using HwndObject = WindowScrape.Types.HwndObject;
using System.Diagnostics;

namespace TeslaX
{
    public static partial class Worker
    {
        public static List<int> EligibleBetween(int a, int b, int off)
        {
            List<int> result = new List<int>();
            int start = (a / 32) * 32 + off + (a % 32 < off ? 0 : 32);
            for (int i = start; i <= b; i += 32)
                result.Add(i);
            return result;
        }

        public static void RowLoop()
        {
            Point tmpoint;
            Screenshot shot;

            int Distance = -1;
            int NewDistance;

            // Blocks in front of character to check.
            int range = 4;

            // Spike handling mechanism.
            Stopwatch SpikeWatch = new Stopwatch();
            bool spike = false;
            // Spike height.
            int sh = 24;
            // Spike length.
            int sl = 150;

            // Input smoothing mechanism.
            Stopwatch InputWatch = new Stopwatch();
            bool KeyDown = false;

            while (Busy)
            {
                shot = new Screenshot(LastKnown.X + (Right ? 0 : -range*32), LastKnown.Y, (range+1)*32, 64);

                tmpoint = shot.GetOffset();
                if (tmpoint != InvalidPoint)
                    Offset = tmpoint;
                else
                {
                    Log("Offset?");
                    continue;
                }

                tmpoint = shot.GetPlayer(Right);
                if (tmpoint != InvalidPoint)
                    LastKnown = tmpoint;
                else
                {
                    Log("Player?");
                    continue;
                }

                List<int> ToCheck;
                NewDistance = -1;
                if (Right)
                {
                    ToCheck = EligibleBetween(LastKnown.X + 32, LastKnown.X + shot.Width - 32, Offset.X).AddInt(-shot.X);
                    for (int x = 0; x < ToCheck.Count; x++)
                        if (shot.HasBlock(ToCheck[x], 0) != BlockState.Air)
                        {
                            NewDistance = (ToCheck[x] + shot.X) - LastKnown.X - 32;
                            break;
                        }
                }
                else
                {
                    ToCheck = EligibleBetween(shot.X - 32, LastKnown.X - 32, Offset.X).AddInt(-shot.X);
                    for(int x = ToCheck.Count - 1; x>=0; x--)
                        if(shot.HasBlock(ToCheck[x], 0) != BlockState.Air)
                        {
                            NewDistance = LastKnown.X - (ToCheck[x] + shot.X) - 32;
                            break;
                        }
                }

                if (NewDistance == -1) {
                    Log("It's -1");
                    continue;
                }

                if (spike)
                {
                    if (SpikeWatch.ElapsedMilliseconds > sl || Math.Abs(NewDistance - Distance) <= sh)
                    {
                        Distance = NewDistance;
                        SpikeWatch.Stop();
                        spike = false;
                    }
                }
                else
                if (Math.Abs(NewDistance - Distance) > sh)
                {
                    SpikeWatch.Restart();
                    spike = true;
                }
                else
                    Distance = NewDistance;

                bool NewKeyDown = Distance > 26; //(Right ? 38 : 0);

                if (InputWatch.ElapsedMilliseconds > 150 && NewKeyDown != KeyDown)
                {
                    InputWatch.Restart();
                    KeyDown = NewKeyDown;
                    //Key.Send(Right ? KeyCode.Right : KeyCode.Left, KeyDown);
                    
                }

                // First iteration only.
                if (!InputWatch.IsRunning)
                    InputWatch.Start();

                Log((KeyDown ? "+" : "-") + Distance.ToString() + (spike ? "S" : ""));
            }

            Restore();
        }
    }
}