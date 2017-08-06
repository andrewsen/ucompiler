using System;
using System.Collections.Generic;

namespace Translator
{
	public class ClassSymbolTable : Dictionary<string, IClassElement>
	{
        public void Add(IClassElement element)
        {
            // TODO: Add logic for methods
            if (base.ContainsKey(element.Name))
            {
                InfoProvider.AddError("Identifier is already in use", ExceptionType.IdentifierInUse, element.DeclarationPosition);
                InfoProvider.AddError("previously declared here", ExceptionType.AdditionalInfo, base[element.Name].DeclarationPosition);
            }

            base.Add(element.Name, element);
        }
	}
}

