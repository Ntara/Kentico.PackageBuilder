// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;

namespace Ntara.PackageBuilder
{
	[AttributeUsage(AttributeTargets.Property)]
	internal sealed class ArgumentAttribute : Attribute
	{
		public ArgumentAttribute(string name, string descriptionResourceName)
		{
			Name = name ?? string.Empty;
			Description = CommonResources.ResourceManager.GetString(descriptionResourceName, CommonResources.Culture) ?? string.Empty;
			ValueName = string.Empty;
		}

		public ArgumentAttribute(string name, string descriptionResourceName, string valueName)
		{
			Name = name ?? string.Empty;
			Description = CommonResources.ResourceManager.GetString(descriptionResourceName, CommonResources.Culture) ?? string.Empty;
			ValueName = valueName ?? string.Empty;
		}

		public string Name { get; }
		public string Description { get; }
		public string ValueName { get; }

		public string Usage
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(ValueName))
				{
					return $"{Name}:<{ValueName}>";
				}

				return Name;
			}
		}
	}
}