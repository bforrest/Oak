﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.SampleClasses;

namespace Oak.Tests.describe_DynamicModel
{
    class confirmation : nspec
    {
        dynamic person;

        bool isValid;

        void before_each()
        {
            person = new Person();

            person.Email = "user@example.com";
            person.EmailConfirmation = "user@example.com";
        }

        void confirming_password_is_entered()
        {
            act = () => isValid = person.IsValid();

            context["given emails match"] = () =>
            {
                before = () =>
                {
                    person.Email = "user@example.com";
                    person.EmailConfirmation = "user@example.com";
                };

                it["is valid"] = () => isValid.should_be_true();
            };

            context["given emails do not match"] = () =>
            {
                before = () =>
                {
                    person.Email = "user@example.com";
                    person.EmailConfirmation = "dd";
                };

                it["is invalid"] = () => isValid.should_be_false();
            };

            it["the confirmation property is not considered for persistance, but is still accessible"] = () =>
            {
                (person.EmailConfirmation as string).should_be("user@example.com");

                (person as Prototype).RespondsTo("EmailConfirmation").should_be_false();
            };

            context["loading property on initialization"] = () =>
            {
                before = () => person = new Person(new { Email = "user@example.com", EmailConfirmation = "user@example.com" });

                it["the confrimation property is not considered for persistance, but is still accessible"] = () =>
                {
                    (person.EmailConfirmation as string).should_be("user@example.com");

                    (person as Prototype).RespondsTo("EmailConfirmation").should_be_false();
                };
            };
        }
    }
}
