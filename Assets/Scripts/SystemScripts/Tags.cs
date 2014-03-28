using UnityEngine;
using System.Collections;

public class Tags : MonoBehaviour
{
	// Game Object Tags
	public const string PLAYER = "Player";
	public const string GAMECONTROLLER = "GameController";
	public const string ENEMY = "Enemy";
	public const string MAININVENTORY = "MainInventory";
	public const string HOLDINGAREA = "HoldingArea";
	public const string INVENTORYITEM = "InventoryItem";
	public const string PREFABLIST = "PrefabList";
	public const string ITEMDROP = "ItemDrop";
	public const string WEAPONSTATS = "WeaponStats";
	public const string SHOPCOSTLIST = "ShopCostList";
	public const string GROUND = "Ground";
	public const string TREASUREDROPPER = "TreasureDropper";
	public const string AMMODROPPER = "AmmoDropper";
	public const string ACTIONCAMERA = "ActionCamera";
	public const string DISTANCEMARKER = "DistanceMarker";
	public const string LEVELENDMARKER = "LevelEndMarker";
	public const string DISCARDAREA = "DiscardArea";
	public const string PLAYERSPAWN = "PlayerSpawn";
	public const string ENEMYSPAWNER = "EnemySpawner";

	// Level IDs
	public const int MAINMENU = 0;
	public const int ORDERINVENTORY = 1;
	public const int CHAOSINVENTORY = 2;
	public const int ACTIONSCENE = 3;
	public const int BREAKAREA = 4;

	// PlayerPref keys
	public const string PREF_ORDERSCORE = "OrderScore";
	public const string PREF_CHAOSSCORE = "ChaosScore";
	public const string PREF_DISTANCE = "Distance";

	// Constant values
	public const float UNIT_SCALING = 32.0f;
}