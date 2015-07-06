﻿/*
	Copyright (c) 2015 Ki

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

using dnlib.DotNet;

namespace dnSpy.BamlDecompiler.Xaml {
	internal class XamlProperty {
		public XamlType DeclaringType { get; private set; }
		public string PropertyName { get; private set; }

		public IMemberDef ResolvedMember { get; set; }

		public XamlProperty(XamlType type, string name) {
			DeclaringType = type;
			PropertyName = name;
		}

		public void TryResolve() {
			if (ResolvedMember != null)
				return;

			var typeDef = DeclaringType.ResolvedType.ResolveTypeDef();
			if (typeDef == null)
				return;

			ResolvedMember = typeDef.FindProperty(PropertyName);
			if (ResolvedMember == null)
				return;

			ResolvedMember = typeDef.FindField(PropertyName + "Property");
			if (ResolvedMember == null)
				return;

			ResolvedMember = typeDef.FindEvent(PropertyName);
			if (ResolvedMember == null)
				return;

			ResolvedMember = typeDef.FindField(PropertyName + "Event");
		}
	}
}