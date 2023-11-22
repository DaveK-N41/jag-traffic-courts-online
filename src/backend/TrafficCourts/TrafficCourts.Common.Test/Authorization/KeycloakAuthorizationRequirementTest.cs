﻿using AutoFixture;
using System;
using TrafficCourts.Common.Authorization;
using Xunit;

namespace TrafficCourts.Common.Test.Authorization
{
    public class KeycloakAuthorizationRequirementTest
    {
        private readonly string _constructorParameterName;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public KeycloakAuthorizationRequirementTest()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            // grab the contructor parameter name
            var constructor = typeof(KeycloakAuthorizationRequirement).GetConstructor(new[] { typeof(string) });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.
            _constructorParameterName = constructor.GetParameters()[0].Name;
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Fact]
        public void will_not_accept_null()
        {
            var actual = Assert.Throws<ArgumentNullException>(() => new KeycloakAuthorizationRequirement(null!));
            Assert.Equal(_constructorParameterName, actual.ParamName);
        }

        [Fact]
        public void will_not_accept_empty_string()
        {
            var actual = Assert.Throws<ArgumentException>(() => new KeycloakAuthorizationRequirement(string.Empty));
            Assert.Equal(_constructorParameterName, actual.ParamName);
        }

        [Fact]
        public void will_not_accept_whitespace()
        {
            var actual = Assert.Throws<ArgumentException>(() => new KeycloakAuthorizationRequirement(" "));
            Assert.Equal(_constructorParameterName, actual.ParamName);
        }

        [Fact]
        public void sets_policy_to_value()
        {
            Fixture fixture = new Fixture();
            var expected = fixture.Create<string>();

            var sut = new KeycloakAuthorizationRequirement(expected);

            Assert.Equal(expected, sut.PolicyName);
        }
    }
}
