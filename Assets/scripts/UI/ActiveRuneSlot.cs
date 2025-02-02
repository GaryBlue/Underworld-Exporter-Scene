﻿using UnityEngine;
using UnityEngine.UI;

public class ActiveRuneSlot : GuiBase_Draggable
{
    /*GUI element for displaying the select spell runes and for casting those selected runes.*/
    private RawImage thisRune;

    private static readonly Texture2D[] runes = new Texture2D[24];
    private static Texture2D blank;

    public override void Start()
    {
        base.Start();
        thisRune = this.GetComponent<RawImage>();
        for (int i = 0; i < 24; i++)
        {
            if (runes[i] == null)
            {
                runes[i] = GameWorldController.instance.ObjectArt.LoadImageAt(232 + i);
                //Resources.Load <Texture2D> (_RES +"/HUD/Runes/rune_" + i.ToString("D2"));					
            }

        }
        if (blank == null)
        {
            blank = Resources.Load<Texture2D>(_RES + "/HUD/Runes/rune_blank");
        }

    }



    public static void UpdateRuneSlots()
    {
        /*Checks the set value on the player and if different display the new rune.*/
        for (int i = 0; i < 3; i++)
        {
            int rune = UWCharacter.Instance.PlayerMagic.GetActiveRune(i);
            if (rune != -1)
            {
                UWHUD.instance.activeRunes[i].thisRune.texture = runes[rune];
            }
            else
            {
                UWHUD.instance.activeRunes[i].thisRune.texture = blank;
            }

        }
    }

    public void OnClick()
    {
        if ((UWHUD.instance.window.JustClicked == true) || (Dragging == true))
        {
            return;
        }
        if ((WindowDetect.InMap == true) || (WindowDetect.WaitingForInput) || (ConversationVM.InConversation) || (Character.InteractionMode == Character.InteractionModeOptions)) { return; }

        if (UWCharacter.Instance.PlayerMagic.ReadiedSpell == "")
        {
            if (UWCharacter.Instance.PlayerMagic.TestSpellCast(UWCharacter.Instance, UWCharacter.Instance.PlayerMagic.GetActiveRune(0), UWCharacter.Instance.PlayerMagic.GetActiveRune(1), UWCharacter.Instance.PlayerMagic.GetActiveRune(2)))
            {
                UWCharacter.Instance.PlayerMagic.CastSpell(UWCharacter.Instance.gameObject, UWCharacter.Instance.PlayerMagic.GetActiveRune(0), UWCharacter.Instance.PlayerMagic.GetActiveRune(1), UWCharacter.Instance.PlayerMagic.GetActiveRune(2), true);
                UWCharacter.Instance.PlayerMagic.ApplySpellCost();
            }
        }
    }
}
