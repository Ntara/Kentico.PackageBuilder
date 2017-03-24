// -----------------------------------------------------------
// Copyright (c) 2017 Ntara, Inc. All rights reserved.
// All code is provided under the MIT license.
// 
// The complete license is located at the project root or
// may be found online at: https://ntara.github.io/license
// -----------------------------------------------------------

using System;
using System.Collections.Generic;

using CMS.Modules.NuGetPackages;

using NuGet;

namespace Ntara.PackageBuilder
{
	/// <summary>
	/// A property provider containing both module metadata and custom property values.
	/// </summary>
	public class ModulePropertyProvider : IPropertyProvider
	{
		private readonly IDictionary<string, string> _properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModulePropertyProvider"/> class.
		/// </summary>
		/// <param name="moduleMetadata">The module metadata for the package.</param>
		/// <param name="properties">An optional dictionary of custom properties.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="moduleMetadata"/> is null.</exception>
		public ModulePropertyProvider(ModulePackageMetadata moduleMetadata, IDictionary<string, string> properties = null)
		{
			if (moduleMetadata == null)
			{
				throw new ArgumentNullException(nameof(moduleMetadata), CommonResources.ArgumentNullException_ModuleMetadata);
			}

			_properties = new Dictionary<string, string>();
			_properties["id"] = moduleMetadata.Id;
			_properties["version"] = moduleMetadata.Version;
			_properties["title"] = moduleMetadata.Title;
			_properties["description"] = moduleMetadata.Description;
			_properties["authors"] = moduleMetadata.Authors;

			// Append optional property values
			if (properties != null)
			{
				foreach (var property in properties)
				{
					// Note: Custom properties cannot overwrite required module metadata
					if (!_properties.ContainsKey(property.Key))
					{
						_properties.Add(property);
					}
				}
			}
		}

		/// <summary>
		/// Retrieves the value associated with the specified <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property to return.</param>
		/// <returns>The value associated with the specified <paramref name="propertyName"/> if found, otherwise null.</returns>
		public object GetPropertyValue(string propertyName)
		{
			string value;

			if (!_properties.TryGetValue(propertyName, out value))
			{
				return null;
			}

			return value;
		}
	}
}