using System;
using System.Collections.Generic;

namespace Translator
{
	public class ClassSymbolTable
	{
		private List<Field> fields = new List<Field>();
        private List<Property> properties = new List<Property>();
        private List<Method> methods = new List<Method>();

		public List<Method> Methods => methods;
		public List<Property> Properties => properties;
		public List<Field> Fields => fields;

		public void Add(IClassElement element)
		{
			if (element is Method)
                AddMethod(element as Method);
            else if (element is Property)
                AddProperty(element as Property);
            else if (element is Field)
                AddField(element as Field);
            else
			    InfoProvider.AddError("WTF. Unsupported class element", ExceptionType.ImpossibleError, element.DeclarationPosition);
		}

        public void AddField(Field field)
		{
            // Checking if identifier exists
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
                IClassElement element = fields.Find(f => f.Name == property.Name) ?? properties.Find(m => m.Name == property.Name);
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

        public IClassElement Find(string name)
        {
            return fields.Find(f => f.Name == name) ?? properties.Find(p => p.Name == name);
        }

        public bool Contains(string name)
        {
            return fields.Exists(f => f.Name == name) || properties.Exists(p => p.Name == name) || methods.Exists(m => m.Name == name);
        }

		private void reportInUse(IClassElement element)
		{
			InfoProvider.AddError("Identifier is already in use", ExceptionType.IdentifierInUse, element.DeclarationPosition);
			InfoProvider.AddError("previously declared here", ExceptionType.AdditionalInfo, element.DeclarationPosition);
		}

        public object FieldIndex(Field field)
        {
            return fields.IndexOf(field);
        }
    }
}

