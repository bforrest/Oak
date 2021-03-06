﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak
{
    public class Changes
    {
        dynamic gemini;

        IDictionary<string, dynamic> originalValues;

        IDictionary<string, dynamic> CurrentValues()
        {
            return gemini.HashOfProperties();
        }

        public Changes(dynamic gemini)
        {
            gemini.SetMember("HasChanged", new DynamicFunctionWithParam(HasChanged));

            gemini.SetMember("Original", new DynamicFunctionWithParam(Original));

            gemini.SetMember("Changes", new DynamicFunctionWithParam(GetChanges));

            originalValues = new Dictionary<string, object>(gemini.HashOfProperties());

            this.gemini = gemini;
        }

        public dynamic GetChanges(dynamic property)
        {
            if (property != null) return ChangesFor(property);

            var dictionary = new Prototype() as IDictionary<string, object>;

            var keys = CurrentValues().Keys.Union(originalValues.Keys).Distinct();

            foreach (var key in keys) if (HasPropertyChanged(key)) dictionary.Add(key, ChangesFor(key));

            return dictionary;
        }

        public dynamic ChangesFor(dynamic property)
        {
            return BeforeAfter(Original(property), Current(property));
        }

        private dynamic BeforeAfter(dynamic before, dynamic after)
        {
            dynamic prototype = new Prototype();

            prototype.Original = before;

            prototype.New = after;

            return prototype;
        }

        public dynamic Original(dynamic property)
        {
            return NullOrValueFor(originalValues, property);
        }

        public dynamic Current(dynamic property)
        {
            return NullOrValueFor(CurrentValues(), property);
        }

        private dynamic NullOrValueFor(IDictionary<string, dynamic> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key)) return null;

            return dictionary[key];
        }

        public dynamic HasChanged(dynamic property)
        {
            if (property != null) return HasPropertyChanged(property);

            return (GetChanges(property) as IDictionary<string, object>).Any();
        }

        public dynamic HasPropertyChanged(dynamic property)
        {
            return Original(property) != Current(property);
        }
    }
}