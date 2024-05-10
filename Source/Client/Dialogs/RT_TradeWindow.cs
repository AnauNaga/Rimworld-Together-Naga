using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using static Shared.CommonEnumerators;

using UnityEngine;
using Verse;

namespace GameClient.Dialogs
{
    public class RT_TradeWindow : Window
    {

        public RT_TradeWindow(bool allowItems = false) {

            ClientValues.ToggleTransfer(true);

            forcePause = true;
            absorbInputAroundWindow = true;

            soundAppear = SoundDefOf.CommsWindow_Open;
            //soundClose = SoundDefOf.CommsWindow_Close;

            closeOnAccept = false;
            closeOnCancel = false;

            PrepareWindow();
        }

        public override void DoWindowContents(Rect Rect)
        {
            
        }


        private void PrepareWindow()
        {
            GetNegotiator();

            GenerateTradeList();

            LoadAllAvailableTradeables();

            SetupTrade();


        }

    }
}
