﻿using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Windows.Input;
using Alphaleonis.Win32.Filesystem;
using ReactiveUI.Fody.Helpers;

namespace DivinityModManager.Models
{
	[DataContract]
	public class DivinityLoadOrderEntry
	{
		[DataMember]
		public string UUID { get; set; }

		[DataMember]
		public string Name { get; set; }
		public bool Missing { get; set; } = false;

		public DivinityLoadOrderEntry Clone()
		{
			return new DivinityLoadOrderEntry() { Name = this.Name, UUID = this.UUID, Missing = this.Missing };
		}
	}

	[DataContract]
	public class DivinityLoadOrder : ReactiveObject
	{
		private string _lastName;

		[Reactive] public string Name { get; set; } 
		[Reactive] public string FilePath { get; set; } 
		[Reactive] public DateTime LastModifiedDate { get; set; }

		[Reactive] public bool IsModSettings { get; set; }

		private readonly ObservableAsPropertyHelper<string> _lastModified;

		public string LastModified => _lastModified.Value;

		[DataMember]
		public List<DivinityLoadOrderEntry> Order { get; set; } = new List<DivinityLoadOrderEntry>();

		public void Add(DivinityModData mod)
		{
			try
			{
				if (Order != null && mod != null)
				{
					if (Order.Count > 0)
					{
						bool alreadyInOrder = false;
						foreach (var x in Order)
						{
							if (x != null && x.UUID == mod.UUID)
							{
								alreadyInOrder = true;
								break;
							}
						}
						if (!alreadyInOrder)
						{
							Order.Add(mod.ToOrderEntry());
						}
					}
					else
					{
						Order.Add(mod.ToOrderEntry());
					}
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error adding mod to order:\n{ex}");
			}
		}

		public void AddRange(IEnumerable<DivinityModData> mods)
		{
			foreach (var mod in mods)
			{
				Add(mod);
			}
		}

		public void Remove(DivinityModData mod)
		{
			try
			{
				if (Order != null && Order.Count > 0 && mod != null)
				{
					DivinityLoadOrderEntry entry = null;
					foreach(var x in Order)
					{
						if(x != null && x.UUID == mod.UUID)
						{
							entry = x;
							break;
						}
					}
					if (entry != null) Order.Remove(entry);
				}
			}
			catch (Exception ex)
			{
				DivinityApp.Log($"Error removing mod from order:\n{ex}");
			}
		}

		public void RemoveRange(IEnumerable<DivinityModData> mods)
		{
			if (Order.Count > 0 && mods != null)
			{
				foreach (var mod in mods)
				{
					Remove(mod);
				}
			}
		}

		public void Sort(Comparison<DivinityLoadOrderEntry> comparison)
		{
			try
			{
				if (Order.Count > 1)
				{
					Order.Sort(comparison);
				}
			}
			catch(Exception ex)
			{
				DivinityApp.Log($"Error sorting order:\n{ex}");
			}
		}

		public void SetOrder(IEnumerable<DivinityLoadOrderEntry> nextOrder)
		{
			Order.Clear();
			Order.AddRange(nextOrder);
		}

		public void SetOrder(DivinityLoadOrder nextOrder)
		{
			Order.Clear();
			Order.AddRange(nextOrder.Order);
		}

		public bool OrderEquals(IEnumerable<string> orderList)
		{
			if(Order.Count > 0)
			{
				return Order.Select(x => x.UUID).SequenceEqual(orderList);
			}
			return false;
		}

		public DivinityLoadOrder Clone()
		{
			return new DivinityLoadOrder()
			{
				Name = this.Name,
				Order = this.Order.ToList(),
				LastModifiedDate = this.LastModifiedDate
			};
		}

		public DivinityLoadOrder()
		{
			this.WhenAnyValue(x => x.Name, (name) => !String.IsNullOrEmpty(name) && name != _lastName).Subscribe(_ =>
			{
				DivinityApp.Events.OnOrderNameChanged(_lastName, Name);
				_lastName = Name;
			});
			_lastModified = this.WhenAnyValue(x => x.LastModifiedDate).Select(x => x.ToString("g")).ToProperty(this, nameof(LastModified));
		}
	}
}
