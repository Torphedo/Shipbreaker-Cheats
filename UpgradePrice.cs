using System;
using UnityEngine;

namespace BBI.Unity.Game
{
	[Serializable]
	public class UpgradePrice
	{
		[SerializeField]
		private CurrencyAsset m_CurrencyAsset;

		[SerializeField]
		private int m_Amount;

		public CurrencyAsset CurrencyAsset => m_CurrencyAsset;

		public int Amount => m_Amount;
	}
}
