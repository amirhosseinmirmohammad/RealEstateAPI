using Elsa;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Services;
using Elsa.Workflows;
using Elsa.Workflows.Models;
using Namotion.Reflection;
using Newtonsoft.Json;
using RealEstateApplication.Services.V1;
using RealEstateCore.Enums;

namespace RealEstateService.ElsaWorkflow
{
    [ActivityDefinition(Category = "Users", Description = "Approve Real Estate", Icon = "fas fa-user-check", Outcomes = new[] { OutcomeNames.Done })]

    public class ApproveActivity : CodeActivity
    {
        [ActivityProperty(Hint = "Real Estate Id for approval")]
        public Input<long> RealEstateId { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            try
            {
                // دریافت Query String از کانتکست HTTP
                var httpContext = context.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
                if (httpContext == null)
                {
                    throw new Exception("HttpContext is not available.");
                }

                var realEstateIdString = httpContext.Request.Query["RealEstateId"].ToString();

                if (string.IsNullOrWhiteSpace(realEstateIdString) || !long.TryParse(realEstateIdString, out var realEstateId))
                {
                    throw new ArgumentException("RealEstateId must be provided and be a valid number.");
                }
                // دریافت وابستگی‌ها از کانتکست
                var _realEstateService = context.GetRequiredService<RealEstatesService>();

                // ذخیره RealEstateId در متغیرهای گردش‌کار
                context.Variables.SetPropertyValue("RealEstateId", 12121212);

                context.CreateVariable("test", 45454554);

                // مرحله اول: تخصیص به مدیر 1 و تایید اولیه
                Console.WriteLine("Assigning to Manager 1 for initial approval...");

                // دریافت ID آگهی از ورودی
                var realEstateId2 = RealEstateId.Get(context);

                string RecJson = JsonConvert.SerializeObject(realEstateId);

                if (realEstateId == 0)
                {
                    throw new ArgumentException("RealEstateId must be provided.");
                }

                var realEstateResponse = await _realEstateService.GetRealEstateByIdAsync(Convert.ToInt32(realEstateId));

                if (realEstateResponse.Data == null)
                {
                    throw new Exception("Real estate not found.");
                }

                var realEstate = realEstateResponse.Data;

                // تخصیص UserId به مدیر 1
                realEstate.UserId = "Manager1UserId";
                realEstate.Status = RealEstateStatus.Archived;
                await _realEstateService.ArchiveRealEstateAsync(Convert.ToInt32(realEstateId));
                Console.WriteLine("Initial archive by Manager 1 completed.");

                // بررسی آرشیو مدیر 1
                if (realEstate.Status != RealEstateStatus.Archived)
                {
                    throw new Exception("Manager 1 did not approve the real estate.");
                }

                // مرحله دوم: تخصیص به مدیر 2 برای تایید نهایی
                Console.WriteLine("Assigning to Manager 2 for final approval...");
                realEstate.UserId = "Manager2UserId";
                await _realEstateService.UpdateRealEstateTimeAsync(Convert.ToInt32(realEstateId));
                Console.WriteLine("Final approval by Manager 2 completed.");

                // تایید نهایی
                realEstate.Status = RealEstateStatus.Active;
                await _realEstateService.UpdateRealEstateTimeAsync(Convert.ToInt32(realEstateId));
                Console.WriteLine("Real estate has been finally approved.");

                // Set output result if needed
                //context.SetResult("RealEstateId: " + realEstateId + " approved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw new ApplicationException($"Error during approval process: {ex.Message}", ex);
            }
        }
    }
}
