using System;
using Carbon.Core;
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

		public int Amount
		{
			get
			{
				if (GlobalOptions.Raw.GetBool("General.FreeUpgrades"))
				{
					return m_Amount = 0;
				}
				return m_Amount;
			}
		}
	}
}
