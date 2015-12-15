using UnityEngine; 
using System.Collections;
using System.Collections.Generic;

public class FieldOperations : MonoBehaviour {
	
	
	public const int FieldsHorCount = 10;
	public const int FieldsVerCount = 10;
	
	GameObject[] mPlaneObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	GameObject[] mRedPlaneObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	GameObject[] mRedDotObjects = new GameObject[FieldsHorCount * FieldsVerCount];
	
	public GameObject PlanePrefab;
	public GameObject RedPlanePrefab;
	public GameObject RedDotPrefab;
	public GameObject DestroyerPrefab;
	public GameObject CruiserPrefab;
	public GameObject BattleshipPrefab;
	public GameObject AircraftCarrierPrefab;
	public GameObject uiPlacing;
	public bool EnemyField;
	public GameObject GameObj;
	public GameObject FirePrefab;
	public GameObject LastCell;
	
	GameOrder mGame;
	AimSelectedHandler mAimSelectHandler;
	
	class FireObject
	{
		public GameObject Object;
		public int X;
		public int Y;
		
		public FireObject(GameObject obj, int x, int y)
		{
			Object = obj;
			X = x;
			Y = y;
		}
	}
	
	
	int mSelectionLength = 0;
	bool mSelectionHorizontal = true;
	List<GameObject> mLastSelected = new List<GameObject>();
	List<GameObject> mShips = new List<GameObject>();
	List<FireObject> mFireList = new List<FireObject>();
	Vector3 mFieldStart;
	Vector3 mFieldEnd;
	float mFieldWidth;
	float mFieldHeight;
	GameObject mPlaceShip;
	bool[,] mBusyCells = new bool[FieldsHorCount, FieldsVerCount];
	bool[,] mAttackedCells = new bool[FieldsHorCount, FieldsVerCount];
	public delegate void AimSelectedHandler(int x, int y);
	
	const float FirePosY = 0.5f;
	

	
	// Use this for initialization
	void Start () {
		
		//������������ �������� ������ � ��������� ����
		Vector3 pos = transform.position;
		Vector3 bounds = GetComponent<Renderer>().bounds.size;
		
		mFieldStart = new Vector3(
			pos.x - bounds.x / 2,
			pos.y,
			pos.z - bounds.z / 2);
		
		mFieldEnd = new Vector3(
			pos.x + bounds.x / 2,
			pos.y,
			pos.z + bounds.z / 2);
		
		mFieldWidth = (mFieldEnd.x - mFieldStart.x) / FieldsHorCount;
		mFieldHeight = (mFieldEnd.z - mFieldStart.z) / FieldsVerCount;

		
		//�������� ��������� ��������������� �������� ��� ���� - ������, ������� �����, �����
		CreateCellObjects(mFieldStart, mFieldEnd);


		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mAttackedCells[i, j] = false;
			}
		}
		
		LastCell.GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		foreach (var obj in mLastSelected)
		{
			obj.GetComponent<MeshRenderer>().enabled = false;
		}
		

		
		//���� ����� ����� ������ �����
		if (mSelectionLength > 0)
		{

			RaycastHit hit;

		}
	}

	// �������� ��������� ��������������� �������� �� ���� 
	void CreateCellObjects(Vector3 start, Vector3 end)
	{
		for (int i = 0; i < FieldsHorCount; ++i)
		{
			for (int j = 0; j < FieldsVerCount; ++j)
			{
				mPlaneObjects[j * FieldsHorCount + i] = CreatePlaneObj(
					i, j, start, end);
			}
		}
	}

	GameObject CreatePlaneObj(int x, int y, Vector3 start, Vector3 end)
	{
		GameObject obj = (GameObject)Instantiate (
			PlanePrefab,
			new Vector3 (x * mFieldWidth + start.x + mFieldWidth / 2, 0.3f, y * mFieldHeight + start.z + mFieldHeight / 2), 
			Quaternion.Euler (-90, 0, 180));
		obj.transform.localScale = new Vector3 (5.1f, 5.1f, 1);
		obj.GetComponent<MeshRenderer> ().enabled = false;
		
		return obj;
	}

	//��������� ���������� �������� ���� type �� ����
	public int GetShipsCountByType(Ship.Type type)
	{
		int res = 0;
		
		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();
			
			if(subship.GetShipType() == type)
			{
				++res;
			}
		}
		
		return res;
	}
	//����������, ��� �� ������� ����������� �� ����
	public bool isAllShipsArePlaced 
	{
		get
		{
			return (GetShipsCountByType(Ship.Type.AircraftCarrier) ==
			        Ship.GetShipsMaxCount(Ship.Type.AircraftCarrier)) &&
				(GetShipsCountByType(Ship.Type.Battleship) ==
				 Ship.GetShipsMaxCount(Ship.Type.Battleship)) &&
					(GetShipsCountByType(Ship.Type.Cruiser) ==
					 Ship.GetShipsMaxCount(Ship.Type.Cruiser)) &&
					(GetShipsCountByType(Ship.Type.Destroyer) ==
					 Ship.GetShipsMaxCount(Ship.Type.Destroyer)) &&
					(mPlaceShip == null);
		}
	}
	
	// ���������� ��������� ��������������� �������� �� ����
	public void RefreshRedPlanes()
	{
		foreach (var plane in mRedPlaneObjects)
		{
			plane.GetComponent<MeshRenderer>().enabled = false;
		}
		
		foreach (var dot in mRedDotObjects)
		{
			dot.GetComponent<MeshRenderer>().enabled = false;
		}
		
		if (mGame.State != GameOrder.GameState.Placing)
		{
			for (int i = 0; i < FieldsHorCount; ++i)
			{
				for (int j = 0; j < FieldsVerCount; ++j)
				{
					if(mAttackedCells[i, j])
					{
						if (isCellBelongsShip(i, j))
						{
							mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
						}
						else
						{
							mRedDotObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
						}
					}
				}
			}
		}
		else
		{
			foreach (var ship in mShips)
			{
				if (ship == mPlaceShip)
					continue;
				
				Ship subship = ship.GetComponent<Ship>();
				
				if (subship.isHorizontal())
				{
					for (int i = subship.GetX() - 1; i <= subship.GetX() + subship.GetShipLength(); ++i)
					{
						for (int j = subship.GetY() - 1; j <= subship.GetY() + 1; ++j)
						{
							if (i >= 0 && j >= 0 &&
							    i < FieldsHorCount && j < FieldsVerCount)
							{
								mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
							}
						}
					}
				}
				else
				{
					for (int i = subship.GetX() - 1; i <= subship.GetX() + 1; ++i)
					{
						for (int j = subship.GetY() - 1; j <= subship.GetY() + subship.GetShipLength(); ++j)
						{
							if (i >= 0 && j >= 0 &&
							    i < FieldsHorCount && j < FieldsVerCount)
							{
								mRedPlaneObjects[j * FieldsHorCount + i].GetComponent<MeshRenderer>().enabled = true;
							}
						}
					}
				}
			}
		}
	}

	// ������� ����� ������ ������ ��� �����
	// ����� ���� ����� �������� �������
	// ��������� ������� ���������� � handler
	public void SelectAim(AimSelectedHandler handler)
	{
		mAimSelectHandler = handler;
		mSelectionLength = 1;
		mSelectionHorizontal = true;
	}
	//����������, ��������� �� ������� �� ������ x, y
	public bool isCellBelongsShip(int x, int y)
	{
		foreach(var ship in mShips)
		{
			Ship subship = ship.GetComponent<Ship>();
			
			if(subship.isHorizontal())
			{
				if(subship.GetX() <= x && 
				   subship.GetX() + subship.GetShipLength() > x &&
				   subship.GetY() == y)
				{
					return true;
				}
			}
			else
			{
				if (subship.GetY() <= y &&
				    subship.GetY() + subship.GetShipLength() > y &&
				    subship.GetX() == x)
				{
					return true;
				}
			}
		}
		
		return false;
	}
	// ��������� ������ x, y
	// � ������������ �������� true - ���� ���� ���������
	// false - ���� ������
	public bool AttackCell(int x, int y)
	{
		mAttackedCells[x, y] = true;
		//RefreshRedPlanes();
		//CheckSinkedShips();
		
		//���������� ��������� ��������� ������
		LastCell.transform.position = new Vector3(
			x * mFieldWidth + mFieldStart.x + mFieldWidth / 2,
			0.1f,
			y * mFieldHeight + mFieldStart.z + mFieldHeight / 2);
		LastCell.GetComponent<MeshRenderer>().enabled = true;
		
		return isCellBelongsShip(x, y);
	}

}