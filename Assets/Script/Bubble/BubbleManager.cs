﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

//
public class BubbleManager : MonoBehaviour {

	public static BubbleManager Instance = null;
	public GameObject bubblePrefab;


	//当前的泡泡
	public List<BubbleUnit> bubbleList = new List<BubbleUnit>();


	SpawnPool pool;


	void Awake()
	{
		Instance = this;
	}

	void Start () {
		pool = PoolManager.Pools["BubblePool"];
		Init ();
	}
	

	void Init()
	{
		for (int i=0; i<60; ++i) {
			CreateNewRandomBubble();
		}

		CreateStoneBubble ();
	}

    public void CreateNewRandomBubble()
	{
		Transform newBubble = pool.Spawn("BubbleUnit");
		newBubble.parent = transform;
		
		int randonID =10+ Random.Range (1, 6);
		BubbleUnit bubble=newBubble.GetComponent<BubbleUnit> ();
		bubble.SetData (randonID);
		bubbleList.Add(bubble);
		
		Vector3 randPos = new Vector3 (-200f+400*Random.value,410f+200*Random.value,0f);
		newBubble.transform.localPosition = randPos;
	}

	void CreateStoneBubble()
	{
		Vector3 startPos = new Vector3 (-210f,-210f, 0);
		float len = 52f;
		for (int i=0; i<5; ++i) {
		   
			Transform newBubble = pool.Spawn("StoneBubbleUnit");
			newBubble.parent = transform;
			
			int randonID =40+ Random.Range (1, 6);
			BubbleUnit bubble=newBubble.GetComponent<BubbleUnit> ();
			bubble.SetData (randonID);
			bubbleList.Add(bubble);

			Vector3 pos = startPos + new Vector3(i*len,0f,0f);
			bubble.transform.localPosition = pos;

		}
	}


	/// <summary>
	/// 触发游戏的消除
	/// </summary>
	/// <param name="bubble">Bubble.</param>
	public void Clean(BubbleUnit bubble)
	{
		List<BubbleUnit> linkList = GetLinkBubble (bubble);

		int recycleCount = 0;

		if (linkList.Count > 2) {
			int i=0;
			while(i<linkList.Count)
			{
				linkList[i].ReduceHitCount();
				if(linkList[i].hitCount <=0)
				{
					recycleCount++;
				}

				RecycleBubble(linkList[i]);
				++i;


			}


			for (int k=0; k<recycleCount; ++k) {
				CreateNewRandomBubble();
			}
		}
	}

	/// <summary>
	/// 回收一个泡泡
	/// </summary>
	/// <param name="bubble">Bubble.</param>
	public void RecycleBubble(BubbleUnit bubble)
	{
		if (bubbleList.Contains (bubble) == false) {
			return;
		}

		if (bubble.hitCount > 0) {
			return;
		}

		EffectPool.Instance.Play("BubbleExplode",bubble.transform.position);
		bubbleList.Remove(bubble);
		pool.Despawn(bubble.transform);
	}


	/// <summary>
	/// 广度优先搜索 查找相连的 泡泡
	/// </summary>
	/// <returns>The link bubble.</returns>
	/// <param name="bubble">Bubble.</param>
	List<BubbleUnit> GetLinkBubble(BubbleUnit bubble)
	{
		List<BubbleUnit> resList = new List<BubbleUnit> ();
		List<BubbleUnit> tempList = new List<BubbleUnit> ();

		tempList.AddRange (bubbleList);

		//remove different bubble
		int index = 0;
		while (index <tempList.Count) {
		    if(IDTool.GetTypeID( tempList[index].ID ) != IDTool.GetTypeID( bubble.ID))
			{
				tempList.RemoveAt(index);
			}else
			{
				index++;
			}
		}

		//get the link bubble of select one
		resList.Add (bubble);
		tempList.Remove (bubble);
		int i = 0;
		while (i<resList.Count) {
			BubbleUnit curBub = resList[i];

			int j=0;
			while(j<tempList.Count)
			{
				if( IsTouch(curBub,tempList[j]) )
				{
					resList.Add(tempList[j]);
					tempList.RemoveAt(j);
				}else
				{
					++j;
				}

			}

			++i;
		   
		}
		return resList;
	}

	/// <summary>
	/// 判断两个泡泡是否相连
	/// </summary>
	/// <returns><c>true</c> if this instance is touch the specified a b; otherwise, <c>false</c>.</returns>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	bool IsTouch(BubbleUnit a, BubbleUnit b)
	{
		float distance = Vector3.Distance (a.transform.localPosition, b.transform.localPosition);

		float radiusA = a.bubCollider.radius;
		float radiusB = b.bubCollider.radius;

		float offset = 5f;

		float calDistance = radiusA + radiusB + offset;
		if (calDistance >= distance) {
			return true;
		} else {
			return false;
		}
	}


	/// <summary>
	/// 特殊泡泡的功能 消除同样颜色的泡泡
	/// </summary>
	public void ClearSameColorBubble(BubbleUnit bubble)
	{
		int typeID = Random.Range (1, 6);

		List<BubbleUnit> colorBubList = new List<BubbleUnit> ();

		for (int i=0; i<this.bubbleList.Count; ++i) {
			BubbleUnit curBub= bubbleList[i];
			int curType = IDTool.GetType(curBub.ID);
			int curTypeID = IDTool.GetTypeID(curBub.ID);

			if( curType !=2 && curTypeID == typeID)
			{
				colorBubList.Add(curBub);
			}
		}

		colorBubList.Add (bubble);

		for (int j=0; j<colorBubList.Count; ++j) {
			RecycleBubble(colorBubList[j]);
		}

	}

	/// <summary>
	/// 特殊泡泡效果 全屏消除
	/// </summary>
	public void CleanAllBubble()
	{
		List<BubbleUnit> tempBubbleList = new List<BubbleUnit> ();
		tempBubbleList.AddRange (bubbleList);

		for (int i=0; i<tempBubbleList.Count; ++i) {
			RecycleBubble(tempBubbleList[i]);
		}
	}
}
