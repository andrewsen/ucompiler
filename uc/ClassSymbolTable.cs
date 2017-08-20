using System;
using System.Collections.Generic;

namespace Translator
{
	public class ClassSymbolTable
	{
		private List<Field> fields;
        private List<Property> properties;
        private List<Method> methods;

		public void Add(IClassElement element)
		{
			if (element is Method)
                AddMethod(element as Method);
            else if (element is Property)
                AddProperty(element as Property);
            else if (element is Field)
                AddField(element as Field);
			InfoProvider.AddError("WTF. Unsupported class element", ExceptionType.ImpossibleError, element.DeclarationPosition);
			//InfoProvider.AddError("Identifier is already in use", ExceptionType.IdentifierInUse, element.DeclarationPosition);
			//InfoProvider.AddError("previously declared here", ExceptionType.AdditionalInfo, base[element.Name].DeclarationPosition);
		}

        public void AddField(Field field)
		{
			if (fields.Exists(f => f.Name == field.Name) || properties.Exists(p => p.Name == field.Name))
			{
				IClassElement element = fields.Find(f => f.Name == field.Name) ?? methods.Find(m => m.Name == field.Name);
				reportInUse(element);
			}
			fields.Add(field);
        }

        public void AddProperty(Property property)
		{
			if (fields.Exists(f => f.Name == property.Name) || properties.Exists(p => p.Name == property.Name))
			{
				IClassElement element = fields.Find(f => f.Name == property.Name) ?? methods.Find(m => m.Name == property.Name);
				reportInUse(element);
			}
			properties.Add(property);
        }

        public void AddMethod(Method method)
        {
            if (fields.Exists(f => f.Name == method.Name) || properties.Exists(p => p.Name == method.Name))
            {
				IClassElement element = fields.Find(f => f.Name == method.Name) ?? properties.Find(p => p.Name == method.Name);
				reportInUse(element);
            }
            methods.Add(method);
		}

		private void reportInUse(IClassElement element)
		{
			InfoProvider.AddError("Identifier is already in use", ExceptionType.IdentifierInUse, element.DeclarationPosition);
			InfoProvider.AddError("previously declared here", ExceptionType.AdditionalInfo, element.DeclarationPosition);
		}
	}
}

