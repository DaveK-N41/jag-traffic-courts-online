﻿using TrafficCourts.Domain.Models;

namespace TrafficCourts.Messaging.MessageContracts
{
    public class DisputeCount
    {
        /// <summary>
        /// Represents the disputant plea on count.
        /// </summary>
        public DisputeCountPleaCode PleaCode { get; set; } = DisputeCountPleaCode.N;

        /// <summary>
        /// Count No
        /// </summary>
        public int CountNo { get; set; }

        /// <summary>
        /// The disputant is requesting time to pay the ticketed amount.
        /// </summary>
        public DisputeCountRequestTimeToPay RequestTimeToPay { get; set; } = DisputeCountRequestTimeToPay.N;

        /// <summary>
        /// The disputant is requesting a reduction of the ticketed amount.
        /// </summary>
        public DisputeCountRequestReduction RequestReduction { get; set; } = DisputeCountRequestReduction.N;

        /// <summary>
        /// Does the want to appear in court?
        /// </summary>
        public DisputeCountRequestCourtAppearance RequestCourtAppearance { get; set; } = DisputeCountRequestCourtAppearance.N;
    }
}
