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
	public const string GAMEMODEINFO = "GameModeInfo";
	public const string GUIOBJECT = "GUI";

	// Level IDs
	public const int MAINMENU = 0;
	public const int ORDERINVENTORY = 1;
	public const int CHAOSINVENTORY = 2;
	public const int ACTIONSCENE = 3;
	public const int BREAKAREA = 4;

	// PlayerPref keys
	public const string PREF_ORDER_TIME_SCORE = "OrderTimeScore";
	public const string PREF_ORDER_MOVE_SCORE = "OrderMoveScore";
	public const string PREF_CHAOS_TIME_SCORE = "ChaosTimeScore";
	public const string PREF_CHAOS_MOVE_SCORE = "ChaosMoveScore";
	public const string PREF_CHALLENGE_TIME = "ChallengeTime_";
	public const string PREF_CHALLENGE_MOVES = "ChallengeMoves_";
	public const string PREF_DISTANCE = "Distance";

	// Constant values
	public const float UNIT_SCALING = 32.0f;

	// Challenge Mode Constants
	public const int CHALLENGE_LEVEL_OFFSET = 4;
	//public const int TOTAL_NUM_LEVELS = 11;
}