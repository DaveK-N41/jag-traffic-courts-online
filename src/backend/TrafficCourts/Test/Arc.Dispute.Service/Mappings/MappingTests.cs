﻿using AutoFixture;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficCourts.Arc.Dispute.Service.Mappings;
using TrafficCourts.Arc.Dispute.Service.Models;
using TrafficCourts.Common;
using Xunit;

namespace TrafficCourts.Test.Arc.Dispute.Service.Mappings
{
    public class MappingTests
    {
        [Fact]
        public void Test_convert_data_from_TcoDisputeTicket_to_ArcFileRecord_list()
        {
            // Arrange
            var fixture = new Fixture();
            var tcoDisputeTicket = fixture.Create<TcoDisputeTicket>();

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            var mapper = mockMapper.CreateMapper();

            // Act
            var actual = mapper.Map<List<ArcFileRecord>>(tcoDisputeTicket);

            // Assert
            Assert.NotNull(actual);
            Assert.NotEmpty(actual);

            foreach (var actualRec in actual)
            {
                Assert.Equal(tcoDisputeTicket.TicketIssuanceDate, actualRec.TransactionDate);
                Assert.Equal(tcoDisputeTicket.TicketFileNumber, actualRec.FileNumber);
                Assert.Equal(tcoDisputeTicket.DriversLicence, actualRec.MvbClientNumber);

                if (actualRec is AdnotatedTicket)
                {
                    AdnotatedTicket actualAdnotatedTicket = (AdnotatedTicket)actualRec;
                    Assert.Equal(tcoDisputeTicket.CitizenName, actualAdnotatedTicket.Name);
                    Assert.Contains(tcoDisputeTicket.TicketDetails, _ => _.Section == actualAdnotatedTicket.Section);
                    Assert.Contains(tcoDisputeTicket.TicketDetails, _ => _.Subsection == actualAdnotatedTicket.Subsection);
                    Assert.Contains(tcoDisputeTicket.TicketDetails, _ => _.Paragraph == actualAdnotatedTicket.Paragraph);
                    Assert.Contains(tcoDisputeTicket.TicketDetails, _ => _.Act == actualAdnotatedTicket.Act);
                    Assert.Contains(tcoDisputeTicket.TicketDetails, _ => _.Amount == actualAdnotatedTicket.OriginalAmount);
                    Assert.Equal(tcoDisputeTicket.IssuingOrganization, actualAdnotatedTicket.Organization);
                    Assert.Equal(tcoDisputeTicket.IssuingLocation, actualAdnotatedTicket.OrganizationLocation);
                }
                else
                {
                    DisputedTicket actualDisputedTicket = (DisputedTicket)actualRec;
                    Assert.Equal(tcoDisputeTicket.CitizenName, actualDisputedTicket.Name);
                    Assert.Equal(tcoDisputeTicket.StreetAddress, actualDisputedTicket.StreetAddress);
                    Assert.Equal(tcoDisputeTicket.City, actualDisputedTicket.City);
                    Assert.Equal(tcoDisputeTicket.Province, actualDisputedTicket.Province);
                    Assert.Equal(tcoDisputeTicket.PostalCode, actualDisputedTicket.PostalCode);
                }
            }  
        }

        /// <summary>
        /// These LegalSection tests really should be in their own test class file
        /// </summary>
        /// <param name="code"></param>
        /// <param name="act"></param>
        /// <param name="section"></param>
        /// <param name="description"></param>
        [Theory]
        [CsvData(@"../../../Arc.Dispute.Service/Data/statutes-test.csv")]
        public void can_parse_LegalSection(int code, string act, string section, string description)
        {
            bool actual = LegalSection.TryParse(section, out LegalSection? legalSection);

            Assert.True(actual);
            Assert.NotNull(legalSection);
            Assert.Equal(section, legalSection!.ToString());
        }

        /// <summary>
        /// These LegalSection tests really should be in their own test class file
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("(a)")]
        [InlineData("x(a)")]
        [InlineData("(a)(i)")]
        public void will_not_parse_invalid_LegalSection(string section)
        {
            bool actual = LegalSection.TryParse(section, out LegalSection? legalSection);
            Assert.False(actual);
            Assert.Null(legalSection);
        }
    }
}
