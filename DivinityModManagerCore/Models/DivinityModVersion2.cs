﻿using Newtonsoft.Json;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager.Models
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DivinityModVersion2 : ReactiveObject
	{
		[Reactive] public ulong Major { get; set; }
		[Reactive] public ulong Minor { get; set; }
		[Reactive] public ulong Revision { get; set; }
		[Reactive] public ulong Build { get; set; }

		[JsonProperty] [Reactive] public string Version { get; set; }

		private ulong versionInt = 0;

		[JsonProperty]
		public ulong VersionInt
		{
			get { return versionInt; }
			set
			{
				value = Math.Max(ulong.MinValue, Math.Min(value, ulong.MaxValue));
				if (versionInt != value)
				{
					ParseInt(versionInt);
					this.RaisePropertyChanged("VersionInt");
				}
			}
		}

		private void UpdateVersion(ulong major, ulong minor, ulong revision, ulong build)
		{
			Version = String.Format("{0}.{1}.{2}.{3}", major, minor, revision, build);
			var nextVersion = ToInt();
			if (nextVersion != versionInt)
			{
				versionInt = ToInt();
				this.RaisePropertyChanged("VersionInt");
			}
		}

		private void UpdateVersion()
		{
			UpdateVersion(Major, Minor, Revision, Build);
		}

		public ulong ToInt()
		{
			return (Major << 55) + (Minor << 47) + (Revision << 31) + Build;
		}

		public override string ToString()
		{
			return String.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Build);
		}

		public void ParseInt(ulong vInt, bool update = true)
		{
			vInt = Math.Max(ulong.MinValue, Math.Min(vInt, ulong.MaxValue));
			if (versionInt != vInt)
			{
				versionInt = vInt;
				this.RaisePropertyChanged("VersionInt");

				Major = versionInt >> 55;
				Minor = (versionInt >> 47) & 0xFF;
				Revision = (versionInt >> 31) & 0xFFFF;
				Build = versionInt & 0x7FFFFFFFUL;
			}
		}

		public static DivinityModVersion2 FromInt(ulong vInt)
		{
			if (vInt == 1 || vInt == 268435456)
			{
				// 1.0.0.0
				vInt = 36028797018963968;
			}
			return new DivinityModVersion2(vInt);
		}

		public static bool operator >(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt > b.VersionInt;
		}

		public static bool operator <(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt < b.VersionInt;
		}

		public static bool operator >=(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt >= b.VersionInt;
		}

		public static bool operator <=(DivinityModVersion2 a, DivinityModVersion2 b)
		{
			return a.VersionInt <= b.VersionInt;
		}

		public DivinityModVersion2()
		{
			this.WhenAnyValue(x => x.Major, x => x.Minor, x => x.Revision, x => x.Build).Subscribe((x) =>
			{
				UpdateVersion();
			});
		}

		public DivinityModVersion2(ulong vInt) : base()
		{
			ParseInt(vInt);
		}

		public DivinityModVersion2(ulong headerMajor, ulong headerMinor, ulong headerRevision, ulong headerBuild) : base()
		{
			Major = headerMajor;
			Minor = headerMinor;
			Revision = headerRevision;
			Build = headerBuild;
		}

		public static readonly DivinityModVersion2 Empty = new DivinityModVersion2(0, 0, 0, 0);
	}
}
