﻿using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDetectUW : WindowDetect
{

    /// <summary>
    /// Is the game using experimental room manager code.
    /// </summary>
    public static bool UsingRoomManager = false;
    Vector3 windowedPosition;
    Vector3 windowedSize;

    public override void Start()
    {
        base.Start();
        JustClicked = false;
        WindowWaitCount = 0;
        switch (_RES)
        {
            case GAME_UW2:
                UWCharacter.Instance.playerCam.rect = new Rect(0.05f, 0.28f, 0.655f, 0.64f);
                break;
            default:
                UWCharacter.Instance.playerCam.rect = new Rect(0.163f, 0.335f, 0.54f, 0.572f);
                break;
        }
    }

    /// <summary>
    /// Cancel all click input for a few seconds.
    /// </summary>
    /// <param name="waitTime">Wait time.</param>
    public void UWWindowWait(float waitTime)
    {
        JustClicked = true;//Prevent catching something I have just thrown.
        WindowWaitCount = waitTime;
    }


    /// <summary>
    /// General Combat UI interface. Controls attack charging
    /// </summary>
    public override void Update()
    {
        if ((UWCharacter.Instance.isRoaming == true) || (UWCharacter.Instance.CurVIT < 0))
        {//No inventory use while using wizard eye spell or dead.
            return;
        }
        if (JustClicked == true)
        {//Wait until the timer has gone down before allowing further clicks
            WindowWaitCount -= Time.deltaTime;
            if (WindowWaitCount <= 0)
            {
                JustClicked = false;
            }
            return;
        }

        //Choose what actions to take.
        switch (Character.InteractionMode)
        {
            case Character.InteractionModeAttack:
                {
                    if (UWCharacter.Instance.PlayerMagic.ReadiedSpell != "")
                    {//Player has spell to fire off first
                        return;
                    }
                    if (UWCharacter.Instance.PlayerCombat.AttackExecuting == true)
                    {//No attacks can be started while executing the last one.
                        return;
                    }
                    if ((CursorInMainWindow == false))
                    {
                        MouseHeldDown = false;
                        UWCharacter.Instance.PlayerCombat.AttackCharging = false;
                    }
                    if ((MouseHeldDown == true))
                    {
                        if (UWCharacter.Instance.PlayerCombat.AttackCharging == false)
                        {//Begin the attack
                            UWCharacter.Instance.PlayerCombat.CombatBegin();
                        }
                        if ((UWCharacter.Instance.PlayerCombat.AttackCharging == true) && (UWCharacter.Instance.PlayerCombat.Charge < 100))
                        {//While still charging increase the charge by the charge rate.
                            UWCharacter.Instance.PlayerCombat.CombatCharging();
                        }
                        return;
                    }
                    else if (UWCharacter.Instance.PlayerCombat.AttackCharging == true)
                    {
                        //Player has been building an attack up and has released it.
                        UWCharacter.Instance.PlayerCombat.ReleaseAttack();
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }


        if ((ContextUIEnabled) && (InventorySlot.Hovering == false))
        {
            //if (CursorInMainWindow)
            //{
            ContextUIMode();
            //}					
        }
    }


    void ContextUIMode()
    {
        Ray ray;
        if (UWCharacter.Instance.MouseLookEnabled == true)
        {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        UWHUD.instance.ContextMenu.text = "";
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, UWCharacter.Instance.GetUseRange()))
        {
            if (hit.transform.gameObject.GetComponent<ObjectInteraction>() != null)
            {
                UWHUD.instance.ContextMenu.text = hit.transform.gameObject.GetComponent<ObjectInteraction>().LookDescriptionContext();
            }
        }
    }

    /// <summary>
    /// Detects if the mouse if over the window
    /// </summary>
    public void OnMouseEnter()
    {
        CursorInMainWindow = true;
    }

    /// <summary>
    /// Detects if the mouse has left the window.
    /// </summary>
    public void OnMouseExit()
    {
        CursorInMainWindow = false;
    }


    /// <summary>
    /// Raises the mouse down event.
    /// </summary>
    /// <param name="evnt">Evnt.</param>
    public void OnMouseDown(BaseEventData evnt)
    {
        PointerEventData pntr = (PointerEventData)evnt;
        OnPress(true, pntr.pointerId);
    }

    /// <summary>
    /// Releases the mouse up event.
    /// </summary>
    /// <param name="evnt">Evnt.</param>
    public void OnMouseUp(BaseEventData evnt)
    {
        PointerEventData pntr = (PointerEventData)evnt;
        OnPress(false, pntr.pointerId);
    }

    protected override void OnPress(bool isPressed, int PtrID)
    {
        if (UWCharacter.Instance.isRoaming == true)
        {//No inventory use while using wizard eye.
            return;
        }
        base.OnPress(isPressed, PtrID);
        if (CursorInMainWindow == false)
        {
            return;
        }
        if (JustClicked == true)
        {
            return;
        }
        switch (Character.InteractionMode)
        {
            case Character.InteractionModePickup:
                ClickEvent(PtrID);
                break;
            default:
                break;
        }
    }

    public void OnClick(BaseEventData evnt)
    {
        PointerEventData pntr = (PointerEventData)evnt;
        OnClick(pntr.pointerId);
    }


    public void OnClick(int ptrID)
    {
        if (UWCharacter.Instance.isRoaming == true)
        {//No inventory use while using wizard eye.
            return;
        }
        if (JustClicked == true)
        {
            return;
        }
        //Cancel out of a small cutscene

        if (UWHUD.instance.CutScenesSmall.anim.SetAnimation.ToUpper() != "ANIM_BASE")
        {
            if ((UWCharacter.Instance.CurVIT > 0))
            {
                UWHUD.instance.CutScenesSmall.anim.SetAnimation = "Anim_Base";
                return;
            }
        }
        switch (Character.InteractionMode)
        {
            case Character.InteractionModePickup:
                break;
            default:
                ClickEvent(ptrID);
                break;
        }
    }

    /// <summary>
    /// Handles click events on the main window
    /// </summary>
    /// <param name="ptrID">Ptr I.</param>
    void ClickEvent(int ptrID)
    {
        if ((UWCharacter.Instance.PlayerMagic.ReadiedSpell != "") || (JustClicked == true))
        {
            //Debug.Log("player has a spell to cast");
            return;
        }

        if ((ContextUIEnabled) && (ContextUIUse) && (CurrentObjectInHand == null))
        {//If context sensitive UI is enabled and it is one of the use modes override the interaction mode.
            if ((object_base.UseAvail) && (ptrID == -1))//Use on left click
            {
                Character.InteractionMode = Character.InteractionModeUse;
            }
            if ((object_base.PickAvail) && (ptrID == -2))//Pickup on right click
            {
                Character.InteractionMode = Character.InteractionModePickup;
            }
            if ((object_base.TalkAvail) && (ptrID == -1))//Talk on left click
            {
                Character.InteractionMode = Character.InteractionModeTalk;
            }
        }
        InteractionModeControl.UpdateNow = true;
        switch (Character.InteractionMode)
        {
            case Character.InteractionModeOptions://Options mode
                return;//do nothing
            case Character.InteractionModeTalk://Talk
                UWCharacter.Instance.TalkMode();
                break;
            case Character.InteractionModePickup://Pickup
                if (CurrentObjectInHand != null)
                {
                    UWWindowWait(1.0f);
                    ThrowObjectInHand();
                }
                else
                {
                    UWCharacter.Instance.PickupMode(ptrID);
                }
                break;
            case Character.InteractionModeLook://look
                UWCharacter.Instance.LookMode();//do nothing
                break;
            case Character.InteractionModeAttack: //attack
                break;
            case Character.InteractionModeUse://Use
                if (CurrentObjectInHand != null)
                {
                    UWCharacter.Instance.UseMode();
                }
                else
                {
                    UWCharacter.Instance.UseMode();
                }
                break;
        }
    }

    /// <summary>
    /// Throws the object in hand along a vector in the 3d view.
    /// </summary>
    protected override void ThrowObjectInHand()
    {
        base.ThrowObjectInHand();
        if (CurrentObjectInHand != null)
        {//The player is holding something
            if (UWCharacter.Instance.playerInventory.JustPickedup == false)//To prevent the click event dropping an object immediately after pickup
            {
                //Determine what is directly in front of the player via a raycast
                //If something is in the way then cancel the drop
                Ray ray;
                if (UWCharacter.Instance.MouseLookEnabled == true)
                {
                    ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                }
                else
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                }

                RaycastHit hit = new RaycastHit();
                float dropRange = 0.5f;
                if (!Physics.Raycast(ray, out hit, dropRange))
                {//No object interferes with the drop
                 //Calculate the force based on how high the mouse is
                    float force = Input.mousePosition.y / Camera.main.pixelHeight * 200;
                    CurrentObjectInHand.Drop();
                    CurrentObjectInHand.UpdateAnimation();
                    GameWorldController.MoveToWorld(CurrentObjectInHand, false);
                    CurrentObjectInHand.transform.parent = GameWorldController.instance.DynamicObjectMarker();
                    var cnt = CurrentObjectInHand.GetComponent<Container>();
                    if (cnt != null)
                    {//Set the picked up flag recursively for container items.
                        Container.SetPickedUpFlag(cnt, false);
                        //Container.SetItemsParent(cnt, GameWorldController.instance.DynamicObjectMarker());
                        //Container.SetItemsPosition(droppedItem.GetComponent<Container>(), UWCharacter.Instance.playerInventory.InventoryMarker.transform.position);
                    }
                    CurrentObjectInHand.transform.position = ray.GetPoint(dropRange - 0.1f);

                    UnFreezeMovement(CurrentObjectInHand);
                    if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y > 0.4f)
                    {//throw if above a certain point in the view port.
                        Vector3 ThrowDir = ray.GetPoint(dropRange) - ray.origin;
                        //Apply the force along the direction.
                        if (CurrentObjectInHand.gameObject.GetComponent<Rigidbody>() != null)
                        {
                            CurrentObjectInHand.gameObject.GetComponent<Rigidbody>().AddForce(ThrowDir * force);
                        }
                    }
                    CurrentObjectInHand = null;
                }
            }
            else
            {
                UWCharacter.Instance.playerInventory.JustPickedup = false;//The next click event will allow dropping.
            }
        }
    }


    /// <summary>
    /// Sets the full screen mode.
    /// </summary>
    public void SetFullScreen()
    {
        FullScreen = true;
        setPositions();

        UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW1, false);
        UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW2, false);
        RectTransform pos = this.GetComponent<RectTransform>();
        windowedPosition = pos.localPosition;
        windowedSize = pos.sizeDelta;
        pos.localPosition = new Vector3(0f, 0f, 0f);
        pos.sizeDelta = new Vector3(0f, 0f);
        UWCharacter.Instance.playerCam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        UWHUD.instance.RefreshPanels(-1);//refresh controls
    }

    /// <summary>
    /// Unsets full screen mode.
    /// </summary>
    public void UnSetFullScreen()
    {
        FullScreen = false;
        setPositions();
        switch (_RES)
        {
            case GAME_UW2:
                UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW1, false);
                UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW2, true);
                break;
            default:
                UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW1, true);
                UWHUD.instance.EnableDisableControl(UWHUD.instance.main_windowUW2, false);
                break;
        }

        RectTransform pos = this.GetComponent<RectTransform>();
        //pos.localPosition = new Vector3(-22f,25f,0f);
        pos.localPosition = windowedPosition;
        //pos.sizeDelta = new Vector3(-148f, -88f);
        pos.sizeDelta = windowedSize;
        switch (_RES)
        {
            case GAME_UW2:
                UWCharacter.Instance.playerCam.rect = new Rect(0.05f, 0.28f, 0.655f, 0.64f);
                break;
            default:
                UWCharacter.Instance.playerCam.rect = new Rect(0.163f, 0.335f, 0.54f, 0.572f);
                break;
        }

        UWHUD.instance.RefreshPanels(-1);//refresh controls
    }


    /// <summary>
    /// Controls the display mode of the mouse cursor and calls switching between full and windowed screen.
    /// </summary>
    void OnGUI()
    {//Controls switching between Mouselook and interaction and sets the cursor icon
        if (GameWorldController.instance.AtMainMenu)
        {
            DrawCursor();
            return;
        }
        if (ConversationVM.InConversation == true)
        {
            ConversationMouseMode();
        }
        else
        {
            if (InMap == false)
            {
                //if ((Event.current.Equals(Event.KeyboardEvent("f")) && (WaitingForInput==false)))
                if ((Event.current.keyCode == GameWorldController.instance.config.ToggleFullScreen) && (WaitingForInput == false) && (Event.current.type == EventType.KeyDown))
                {//Toggle full screen.
                    if (FullScreen == true)
                    {
                        UnSetFullScreen();
                    }
                    else
                    {
                        SetFullScreen();
                    }
                }
                //if ((Event.current.Equals(Event.KeyboardEvent("t"))) && (WaitingForInput==false))
                if ((Event.current.keyCode == GameWorldController.instance.config.TrackSkill) && (WaitingForInput == false) && (Event.current.type == EventType.KeyDown))
                //if ( ( Input.GetKey(KeyCode.T) ) && (WaitingForInput==false))
                {//Tracking skill
                    TryTracking();
                }


                //if ((Event.current.Equals(Event.KeyboardEvent("e"))) && (WaitingForInput==false))
                if ((Event.current.keyCode == GameWorldController.instance.config.ToggleMouseLook) && (WaitingForInput == false) && (Event.current.type == EventType.KeyDown))
                {
                    if (Character.InteractionMode != Character.InteractionModeOptions)
                    {
                        if (UWCharacter.Instance.MouseLookEnabled == false)
                        {//Switch to mouse look.
                            SwitchToMouseLook();
                        }
                        else
                        {
                            SwitchFromMouseLook();
                        }
                    }
                }
            }


            if (UWCharacter.Instance.MouseLookEnabled == false)
            {
                //DrawCursor();
                // UWHUD.instance.MessageScroll.Add(Time.time.ToString());
            }
            else
            {
                //if (UWHUD.instance.MouseLookCursor.texture.name != UWHUD.instance.CursorIcon.name)	
                //{
                // X UWHUD.instance.MouseLookCursor.texture = UWHUD.instance.CursorIcon;
                //}

                //Added due to unity bug where mouse is offscreen!!!!
                //UGH!!!
                //WHen not in combat or with a readied spell.
                if ((Character.InteractionMode != Character.InteractionModeAttack) && (UWCharacter.Instance.PlayerMagic.ReadiedSpell == ""))
                {
                    if (JustClicked == false)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            CursorInMainWindow = true;
                            OnPress(true, -1);
                        }
                        if (Input.GetMouseButtonDown(1))
                        {
                            CursorInMainWindow = true;
                            OnPress(true, -2);
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            CursorInMainWindow = true;
                            OnPress(false, -1);
                            UWWindowWait(1.0f);
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            CursorInMainWindow = true;
                            OnPress(false, -2);
                            UWWindowWait(1.0f);
                        }
                        if (Input.GetMouseButton(0))
                        {
                            CursorInMainWindow = true;
                            OnClick(-1);
                            UWWindowWait(1.0f);
                        }
                        if (Input.GetMouseButton(1))
                        {
                            CursorInMainWindow = true;
                            OnClick(-2);
                            UWWindowWait(1.0f);
                        }
                    }
                }
                else
                {//Combat mouse clicks
                    CursorInMainWindow = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        CursorInMainWindow = true;
                        //OnClick(-1);
                        OnPress(true, -1);
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        CursorInMainWindow = true;
                        OnPress(true, -2);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        CursorInMainWindow = true;
                        OnPress(false, -1);

                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        CursorInMainWindow = true;
                        OnPress(false, -2);
                    }
                }
            }
        }
    }

    public void DrawCursor()
    {
        //if ((WindowDetectUW.InMap == true) && (MapInteraction.InteractionMode == 2))
        //{
        //    CursorPosition.center = MapInteraction.CursorPos + MapInteraction.caretAdjustment;
        //}
        //else
        //{
        //    CursorPosition.center = Event.current.mousePosition;
        //}
        //GUI.DrawTexture(CursorPosition, UWHUD.instance.CursorIcon);
        //Cursor.SetCursor(UWHUD.instance.CursorIcon, new Vector2(0.5f,0.5f), CursorMode.Auto);
        // Cursor.SetCursor(UWHUD.instance.CursorIcon, new Vector2(UWHUD.instance.CursorIcon.width/2, UWHUD.instance.CursorIcon.height / 2), CursorMode.Auto);        
    }

    public static void SwitchToMouseLook()
    {
        UWCharacter.Instance.YAxis.enabled = true;
        UWCharacter.Instance.XAxis.enabled = true;
        UWCharacter.Instance.MouseLookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UWHUD.instance.MouseLookCursor.texture = UWHUD.instance.CursorIcon;
    }

    public static void SwitchFromMouseLook()
    {
        UWCharacter.Instance.XAxis.enabled = false;
        UWCharacter.Instance.YAxis.enabled = false;
        UWCharacter.Instance.MouseLookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        if (!GameWorldController.instance.AtMainMenu)
        {
            //Cursor.visible = true;            
        }
        UWHUD.instance.MouseLookCursor.texture = UWHUD.instance.CursorIconBlank;
        //UWHUD.instance.MouseLookCursor.texture=UWHUD.instance.CursorIcon;
    }

    void ConversationMouseMode()
    {
        UWCharacter.Instance.XAxis.enabled = false;
        UWCharacter.Instance.YAxis.enabled = false;
        UWCharacter.Instance.MouseLookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        CursorPosition.center = Event.current.mousePosition;
        GUI.DrawTexture(CursorPosition, UWHUD.instance.CursorIcon);
        UWHUD.instance.MouseLookCursor.texture = UWHUD.instance.CursorIconBlank;
    }

    /// <summary>
    /// Tries the tracking skill to detect nearby monsters
    /// </summary>
    void TryTracking()
    {
        bool SkillSucess = Skills.TrySkill(Skills.SkillTrack, Skills.DiceRoll(0, 30));
        int skillLevel = Skills.GetSkill(Skills.SkillTrack);
        Debug.Log("Track test = " + SkillSucess);
        Skills.TrackMonsters(this.gameObject, (float)skillLevel / 3, SkillSucess);
    }
}