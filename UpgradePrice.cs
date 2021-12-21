using System;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000571 RID: 1393
	[Serializable]
	public class UpgradePrice
	{
		// Token: 0x17000823 RID: 2083
		// (get) Token: 0x06001B2D RID: 6957 RVA: 0x00072E8D File Offset: 0x0007108D
		public CurrencyAsset CurrencyAsset
		{
			get
			{
				return this.m_CurrencyAsset;
			}
		}

		// Token: 0x17000824 RID: 2084
		// (get) Token: 0x06001B2E RID: 6958 RVA: 0x00072E95 File Offset: 0x00071095
		public int Amount
		{
			get
			{
				return this.m_Amount;
			}
		}

		// Token: 0x04001348 RID: 4936
		[SerializeField]
		private CurrencyAsset m_CurrencyAsset;

		// Token: 0x04001349 RID: 4937
		[SerializeField]
		private int m_Amount;
	}
}
