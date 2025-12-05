using UnityEngine;

public class ItemHealthController : Item {
    [SerializeField] private GCEnumManager.ITEM_TYPE itemType;
    
    
    protected override void ItemActivate() {
        CargunShipManager.OnBeffItemGet.Invoke(this.itemType);
    }

    protected override void ItemActivate(GCEnumManager.TURRET_TYPE turretId) { 
        return;
    }
}