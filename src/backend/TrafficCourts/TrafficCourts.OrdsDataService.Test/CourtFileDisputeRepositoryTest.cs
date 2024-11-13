using Microsoft.Extensions.DependencyInjection;
using TrafficCourts.OrdsDataService.Tco;

namespace TrafficCourts.OrdsDataService.Test;

public class CourtFileDisputeRepositoryTest : OrdsDataServiceTest
{
    [Fact(Skip = SkipReason)]
    public async Task get_tco_disputes_with_no_parameters()
    {
        var repository = _serviceProvider.GetRequiredService<IDisputeCaseFileSummaryRepository>();

        var values = await repository.GetListAsync(null, CancellationToken.None);
        Assert.NotNull(values?.Rows);
        Assert.NotEmpty(values.Rows);

        Assert.Equal(0, values.Offset);
        Assert.Equal(25, values.Fetch);
        Assert.Equal(28, values.TotalRows);
    }


    [Fact(Skip = SkipReason)]
    public async Task get_tco_disputes_page_two()
    {
        var repository = _serviceProvider.GetRequiredService<IDisputeCaseFileSummaryRepository>();

        Dictionary<string, string> properties = new()
        {
            { "offset_rows", "25" }
        };

        var values = await repository.GetListAsync(properties, CancellationToken.None);
        Assert.NotNull(values?.Rows);
        Assert.NotEmpty(values.Rows);

        Assert.Equal(25, values.Offset);
        Assert.Equal(25, values.Fetch);
        Assert.Equal(28, values.TotalRows);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_tco_disputes_page_three()
    {
        var repository = _serviceProvider.GetRequiredService<IDisputeCaseFileSummaryRepository>();

        Dictionary<string, string> properties = new()
        {
            { "offset_rows", "50" }
        };

        var values = await repository.GetListAsync(properties, CancellationToken.None);
        Assert.NotNull(values?.Rows);
        Assert.Empty(values.Rows);

        Assert.Equal(50, values.Offset);
        Assert.Equal(25, values.Fetch);
        Assert.Equal(28, values.TotalRows);
    }


    /*
    include
    dispute_id_eq
    dispute_id_in

    submitted_dt_ge
    submitted_dt_gt
    submitted_dt_le
    submitted_dt_lt

    jj_decision_dt_ge
    jj_decision_dt_gt
    jj_decision_dt_le
    jj_decision_dt_lt

    // tco_disputes.to_be_heard_at_agen_id
    courthouse_agen_id_eq
    courthouse_agen_id_in
    courthouse_agen_id_not_in

    // appearance agency (requires include=appearances)
    appr_ctrm_agen_id_eq
    appr_ctrm_agen_id_in
    appr_ctrm_agen_id_not_in

    // appearance room (requires include=appearances)
    appr_ctrm_room_cd_eq
    appr_ctrm_room_cd_in

    ticket_number_txt_eq
    prof_surname_nm_eq
    prof_given_1_nm_eq
    vtc_assigned_to_eq
    jj_assigned_to_eq
    occam_violation_ticket_upld_id_eq

    dispute_status_type_cd_eq
    dispute_status_type_cd_in
    dispute_status_type_cd_not_in
    */
}