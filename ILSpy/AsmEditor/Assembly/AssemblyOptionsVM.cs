﻿/*
    Copyright (C) 2014-2015 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;
using System.Windows.Input;
using dnlib.DotNet;
using ICSharpCode.ILSpy.AsmEditor.ViewHelpers;

namespace ICSharpCode.ILSpy.AsmEditor.Assembly
{
	enum AsmProcArch
	{
		None = (int)((uint)AssemblyAttributes.PA_None >> (int)AssemblyAttributes.PA_Shift),
		MSIL = (int)((uint)AssemblyAttributes.PA_MSIL >> (int)AssemblyAttributes.PA_Shift),
		x86 = (int)((uint)AssemblyAttributes.PA_x86 >> (int)AssemblyAttributes.PA_Shift),
		IA64 = (int)((uint)AssemblyAttributes.PA_IA64 >> (int)AssemblyAttributes.PA_Shift),
		AMD64 = (int)((uint)AssemblyAttributes.PA_AMD64 >> (int)AssemblyAttributes.PA_Shift),
		ARM = (int)((uint)AssemblyAttributes.PA_ARM >> (int)AssemblyAttributes.PA_Shift),
		Reserved = 6,
		NoPlatform = (int)((uint)AssemblyAttributes.PA_NoPlatform >> (int)AssemblyAttributes.PA_Shift),
	}

	enum AsmContType
	{
		Default = (int)((uint)AssemblyAttributes.ContentType_Default >> 9),
		WindowsRuntime = (int)((uint)AssemblyAttributes.ContentType_WindowsRuntime >> 9),
	}

	sealed class AssemblyOptionsVM : INotifyPropertyChanged, IDataErrorInfo
	{
		readonly AssemblyOptions options;
		readonly AssemblyOptions origOptions;

		public IOpenPublicKeyFile OpenPublicKeyFile {
			set { openPublicKeyFile = value; }
		}
		IOpenPublicKeyFile openPublicKeyFile;

		public ICommand OpenPublicKeyFileCommand {
			get { return openPublicKeyFileCommand ?? (openPublicKeyFileCommand = new RelayCommand(a => OnOpenPublicKeyFile())); }
		}
		ICommand openPublicKeyFileCommand;

		public ICommand ReinitializeCommand {
			get { return reinitializeCommand ?? (reinitializeCommand = new RelayCommand(a => Reinitialize())); }
		}
		ICommand reinitializeCommand;

		static readonly EnumVM[] hashAlgorithmList = EnumVM.Create(typeof(AssemblyHashAlgorithm));
		public EnumListVM HashAlgorithm {
			get { return hashAlgorithmVM; }
		}
		readonly EnumListVM hashAlgorithmVM;

		public UInt16VM VersionMajor {
			get { return versionMajor; }
		}
		UInt16VM versionMajor;

		public UInt16VM VersionMinor {
			get { return versionMinor; }
		}
		UInt16VM versionMinor;

		public UInt16VM VersionBuild {
			get { return versionBuild; }
		}
		UInt16VM versionBuild;

		public UInt16VM VersionRevision {
			get { return versionRevision; }
		}
		UInt16VM versionRevision;

		public AssemblyAttributes Flags {
			get {
				return (flags & ~(AssemblyAttributes.PA_Mask | AssemblyAttributes.ContentType_Mask) |
					((AssemblyAttributes)((uint)(AsmProcArch)ProcessArchitecture.SelectedItem << (int)AssemblyAttributes.PA_Shift) & AssemblyAttributes.PA_Mask) |
					((AssemblyAttributes)((uint)(AsmContType)ContentType.SelectedItem << 9) & AssemblyAttributes.ContentType_Mask));
			}
			set {
				if (flags != value) {
					flags = value;
					OnPropertyChanged("AssemblyFullName");
					OnPropertyChanged("Flags");
					OnPropertyChanged("FlagsPublicKey");
					OnPropertyChanged("PA_Specified");
					OnPropertyChanged("Retargetable");
					OnPropertyChanged("EnableJITcompileTracking");
					OnPropertyChanged("DisableJITcompileOptimizer");
					ProcessArchitecture.SelectedItem = (AsmProcArch)((uint)(flags & AssemblyAttributes.PA_Mask) >> (int)AssemblyAttributes.PA_Shift);
					ContentType.SelectedItem = (AsmContType)((uint)(flags & AssemblyAttributes.ContentType_Mask) >> 9);
				}
			}
		}
		AssemblyAttributes flags;

		public bool FlagsPublicKey {
			get { return GetFlagValue(AssemblyAttributes.PublicKey); }
			set { SetFlagValue(AssemblyAttributes.PublicKey, value); }
		}

		public bool PA_Specified {
			get { return GetFlagValue(AssemblyAttributes.PA_Specified); }
			set { SetFlagValue(AssemblyAttributes.PA_Specified, value); }
		}

		public bool Retargetable {
			get { return GetFlagValue(AssemblyAttributes.Retargetable); }
			set { SetFlagValue(AssemblyAttributes.Retargetable, value); }
		}

		public bool EnableJITcompileTracking {
			get { return GetFlagValue(AssemblyAttributes.EnableJITcompileTracking); }
			set { SetFlagValue(AssemblyAttributes.EnableJITcompileTracking, value); }
		}

		public bool DisableJITcompileOptimizer {
			get { return GetFlagValue(AssemblyAttributes.DisableJITcompileOptimizer); }
			set { SetFlagValue(AssemblyAttributes.DisableJITcompileOptimizer, value); }
		}

		bool GetFlagValue(AssemblyAttributes flag)
		{
			return (Flags & flag) != 0;
		}

		void SetFlagValue(AssemblyAttributes flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}

		internal static readonly EnumVM[] processArchList = EnumVM.Create(typeof(AsmProcArch));
		public EnumListVM ProcessArchitecture {
			get { return processArchitectureVM; }
		}
		readonly EnumListVM processArchitectureVM = new EnumListVM(processArchList);

		internal static readonly EnumVM[] contentTypeList = EnumVM.Create(typeof(AsmContType));
		public EnumListVM ContentType {
			get { return contentTypeVM; }
		}
		readonly EnumListVM contentTypeVM;

		public HexStringVM PublicKey {
			get { return publicKey; }
		}
		HexStringVM publicKey;

		public string Name {
			get { return name; }
			set {
				if (name != value) {
					name = value;
					OnPropertyChanged("Name");
					OnPropertyChanged("AssemblyFullName");
				}
			}
		}
		string name;

		public string Culture {
			get { return culture; }
			set {
				if (culture != value) {
					culture = value;
					OnPropertyChanged("Culture");
					OnPropertyChanged("AssemblyFullName");
				}
			}
		}
		string culture;

		public string AssemblyFullName {
			get {
				var asm = new AssemblyNameInfo();
				asm.HashAlgId = (AssemblyHashAlgorithm)HashAlgorithm.SelectedItem;
				int major = VersionMajor.HasError ? 0 : VersionMajor.Value;
				int minor = VersionMinor.HasError ? 0 : VersionMinor.Value;
				int build = VersionBuild.HasError ? 0 : VersionBuild.Value;
				int revision = VersionRevision.HasError ? 0 : VersionRevision.Value;
				asm.Version = new Version(major, minor, build, revision);
				asm.Attributes = Flags;
				asm.PublicKeyOrToken = new PublicKey(publicKey.HasError ? new byte[0] : publicKey.Value);
				asm.Name = Name;
				asm.Culture = Culture;
				return asm.ToString();
			}
		}

		public AssemblyOptionsVM()
			: this(new AssemblyOptions())
		{
		}

		public AssemblyOptionsVM(AssemblyOptions options)
		{
			this.options = new AssemblyOptions();
			this.origOptions = options;
			this.hashAlgorithmVM = new EnumListVM(hashAlgorithmList, () => OnPropertyChanged("AssemblyFullName"));
			this.contentTypeVM = new EnumListVM(contentTypeList, () => OnPropertyChanged("AssemblyFullName"));
			this.versionMajor = new UInt16VM(a => { HasErrorUpdated(); OnPropertyChanged("AssemblyFullName"); }) { UseDecimal = true };
			this.versionMinor = new UInt16VM(a => { HasErrorUpdated(); OnPropertyChanged("AssemblyFullName"); }) { UseDecimal = true };
			this.versionBuild = new UInt16VM(a => { HasErrorUpdated(); OnPropertyChanged("AssemblyFullName"); }) { UseDecimal = true };
			this.versionRevision = new UInt16VM(a => { HasErrorUpdated(); OnPropertyChanged("AssemblyFullName"); }) { UseDecimal = true };
			this.publicKey = new HexStringVM(a => { HasErrorUpdated(); OnPropertyChanged("AssemblyFullName"); }) { UpperCaseHex = false };
			Reinitialize();
		}

		void Reinitialize()
		{
			InitializeFrom(origOptions);
		}

		public AssemblyOptions CreateAssemblyOptions()
		{
			return CopyTo(new AssemblyOptions());
		}

		void InitializeFrom(AssemblyOptions options)
		{
			HashAlgorithm.SelectedItem = options.HashAlgorithm;
			VersionMajor.Value = checked((ushort)options.Version.Major);
			VersionMinor.Value = checked((ushort)options.Version.Minor);
			VersionBuild.Value = checked((ushort)options.Version.Build);
			VersionRevision.Value = checked((ushort)options.Version.Revision);
			Flags = options.Attributes;
			ProcessArchitecture.SelectedItem = (AsmProcArch)((uint)(options.Attributes & AssemblyAttributes.PA_Mask) >> (int)AssemblyAttributes.PA_Shift);
			ContentType.SelectedItem = (AsmContType)((uint)(options.Attributes & AssemblyAttributes.ContentType_Mask) >> 9);
			PublicKey.Value = options.PublicKey.Data;
			Name = options.Name;
			Culture = options.Culture;
		}

		AssemblyOptions CopyTo(AssemblyOptions options)
		{
			options.HashAlgorithm = (AssemblyHashAlgorithm)HashAlgorithm.SelectedItem;
			options.Version = new Version(VersionMajor.Value, VersionMinor.Value, VersionBuild.Value, VersionRevision.Value);
			options.Attributes = Flags;
			options.PublicKey = new PublicKey(PublicKey.Value);
			options.Name = Name;
			options.Culture = Culture;
			return options;
		}

		void OnOpenPublicKeyFile()
		{
			if (openPublicKeyFile == null)
				throw new InvalidOperationException();
			var newPublicKey = openPublicKeyFile.Open();
			if (newPublicKey == null)
				return;
			publicKey.Value = newPublicKey.Data;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		public string Error {
			get { throw new NotImplementedException(); }
		}

		public string this[string columnName] {
			get {
				HasErrorUpdated();
				return Verify(columnName);
			}
		}

		string Verify(string columnName)
		{
			return string.Empty;
		}

		void HasErrorUpdated()
		{
			OnPropertyChanged("HasError");
			OnPropertyChanged("HasNoError");
		}

		public bool HasNoError {
			get { return !HasError; }
		}

		public bool HasError {
			get {
				return versionMajor.HasError ||
					versionMinor.HasError ||
					versionBuild.HasError ||
					versionRevision.HasError ||
					publicKey.HasError;
			}
		}
	}
}