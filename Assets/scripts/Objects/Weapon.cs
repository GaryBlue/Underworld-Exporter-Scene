﻿public class Weapon : Equipment
{

    public override bool use()
    {
        if (CurrentObjectInHand == null)
        {
            if (((this.objInt().inventorySlot == 7) && (UWCharacter.Instance.isLefty == false)) || ((this.objInt().inventorySlot == 8) && (UWCharacter.Instance.isLefty == true)))
            {
                if (Character.InteractionMode == Character.InteractionModeAttack)
                {
                    Character.InteractionMode = Character.InteractionModeUse;
                }
                else
                {
                    Character.InteractionMode = Character.InteractionModeAttack;
                }
            }
            InteractionModeControl.UpdateNow = true;
            return true;
        }
        else
        {
            return ActivateByObject(CurrentObjectInHand);
        }
        //return false;
    }

    public override bool EquipEvent(short slotNo)
    {
        //UWCharacter.Instance.PlayerCombat.currWeapon= this;
        if (((slotNo == 7) && (UWCharacter.Instance.isLefty == false)) || ((slotNo == 8) && (UWCharacter.Instance.isLefty == true)))
        {
            if (this.GetComponent<WeaponRanged>() != null)
            {
                UWCharacter.Instance.PlayerCombat.currWeaponRanged = (WeaponRanged)this;
            }
            if (this.GetComponent<WeaponMelee>() != null)
            {
                UWCharacter.Instance.PlayerCombat.currWeapon = (WeaponMelee)this;
            }

        }
        return true;
    }

    public override bool UnEquipEvent(short slotNo)
    {
        if (((slotNo == 7) && (UWCharacter.Instance.isLefty == false)) || ((slotNo == 8) && (UWCharacter.Instance.isLefty == true)))
        {
            UWCharacter.Instance.PlayerCombat.currWeapon = null;
            UWCharacter.Instance.PlayerCombat.currWeaponRanged = null;
        }
        return false;
    }


}
