using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NomiWrite.Payment.API.Controllers;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Application.UnitTests.Persistence;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.Application.UnitTests;

public class PaymentIpnControllerTests
{
    private const string VnPaySecret = "test-hash-secret";
    private const string MomoAccessKey = "access-key-test";
    private const string MomoSecretKey = "secret-key-test";
    private const string MomoPartnerCode = "MOMO123";

    private static PaymentController BuildController(TestPaymentDbContext db, IPublishEndpoint publish)
    {
        var vnPay = new VnPayGatewayService(
            Options.Create(new VnPaySettings
            {
                TmnCode = "VNP12345",
                HashSecret = VnPaySecret,
                BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
                ReturnUrl = "http://localhost:3000/payment/vnpay/return"
            }),
            NullLogger<VnPayGatewayService>.Instance);

        var momo = new MomoGatewayService(
            Options.Create(new MomoSettings
            {
                PartnerCode = MomoPartnerCode,
                AccessKey = MomoAccessKey,
                SecretKey = MomoSecretKey,
                Endpoint = "https://test-payment.momo.vn/v2/gateway/api/create",
                RedirectUrl = "http://localhost:3000/payment/momo/return",
                IpnUrl = "http://localhost:3000/payment/momo/ipn"
            }),
            new HttpClient(),
            NullLogger<MomoGatewayService>.Instance);

        return new PaymentController(
            Substitute.For<IPaymentService>(),
            vnPay,
            momo,
            db,
            publish,
            NullLogger<PaymentController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static PaymentOrder SeedPayment(
        TestPaymentDbContext db,
        PaymentProvider provider,
        decimal amount = 100_000m,
        PaymentStatus status = PaymentStatus.Pending)
    {
        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = amount,
            Currency = "VND",
            Provider = provider,
            Status = status,
            OrderReference = $"PAY-{Guid.NewGuid():N}".ToUpperInvariant(),
            PlanId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    private static IReadOnlyList<PaymentCompletedEvent> CompletedEvents(IPublishEndpoint publish)
        => publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<PaymentCompletedEvent>()
            .ToList();

    private static void SetVnPayQuery(PaymentController controller, IDictionary<string, string> parameters)
    {
        var query = string.Join("&", parameters.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        controller.ControllerContext.HttpContext.Request.QueryString = new QueryString($"?{query}");
    }

    private static SortedDictionary<string, string> VnPaySuccessParams(PaymentOrder payment, decimal? amount = null)
    {
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_TmnCode"] = "VNP12345",
            ["vnp_Amount"] = Convert.ToInt64((amount ?? payment.Amount) * 100).ToString(),
            ["vnp_TxnRef"] = payment.OrderReference,
            ["vnp_TransactionNo"] = "987654",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionStatus"] = "00"
        };
        parameters["vnp_SecureHash"] = HmacSha512(VnPaySecret, BuildUrlEncodedInput(parameters));
        return parameters;
    }

    private static MomoIpnPayload MomoSuccessPayload(PaymentOrder payment, long? amount = null)
    {
        var payload = new MomoIpnPayload
        {
            PartnerCode = MomoPartnerCode,
            OrderId = payment.OrderReference,
            RequestId = "req-1",
            Amount = amount ?? Convert.ToInt64(payment.Amount),
            OrderInfo = $"Thanh toan don hang {payment.OrderReference}",
            OrderType = "momo_wallet",
            TransId = 555555,
            ResultCode = 0,
            Message = "Successful.",
            PayType = "qr",
            ResponseTime = 1720000000000,
            ExtraData = string.Empty
        };

        payload.Signature = HmacSha256(MomoSecretKey, MomoRawSignature(payload));
        return payload;
    }

    [Fact]
    public async Task VnPayIpn_ValidSuccess_CompletesAndDuplicateDoesNotRepublish()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var payment = SeedPayment(db, PaymentProvider.VNPay);
        var controller = BuildController(db, publish);
        SetVnPayQuery(controller, VnPaySuccessParams(payment));

        var first = await controller.HandleVnPayIpn();
        var second = await controller.HandleVnPayIpn();

        first.Should().BeOfType<OkObjectResult>();
        second.Should().BeOfType<OkObjectResult>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        db.PaymentTransactions.Should().ContainSingle(t => t.PaymentId == payment.Id);
        CompletedEvents(publish).Should().ContainSingle(e => e.PaymentId == payment.Id);
    }

    [Fact]
    public async Task VnPayIpn_AmountMismatch_DoesNotCompleteOrPublish()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var payment = SeedPayment(db, PaymentProvider.VNPay);
        var controller = BuildController(db, publish);
        SetVnPayQuery(controller, VnPaySuccessParams(payment, amount: payment.Amount + 1));

        var result = await controller.HandleVnPayIpn();

        result.Should().BeOfType<OkObjectResult>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
        db.PaymentTransactions.Should().BeEmpty();
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task MomoIpn_ValidSuccess_CompletesAndDuplicateDoesNotRepublish()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var payment = SeedPayment(db, PaymentProvider.Momo);
        var controller = BuildController(db, publish);
        var payload = MomoSuccessPayload(payment);

        var first = await controller.HandleMomoIpn(payload);
        var second = await controller.HandleMomoIpn(payload);

        first.Should().BeOfType<NoContentResult>();
        second.Should().BeOfType<NoContentResult>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
        db.PaymentTransactions.Should().ContainSingle(t => t.PaymentId == payment.Id);
        CompletedEvents(publish).Should().ContainSingle(e => e.PaymentId == payment.Id);
    }

    [Fact]
    public async Task MomoIpn_AmountMismatch_DoesNotCompleteOrPublish()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var payment = SeedPayment(db, PaymentProvider.Momo);
        var controller = BuildController(db, publish);
        var payload = MomoSuccessPayload(payment, amount: Convert.ToInt64(payment.Amount) + 1);

        var result = await controller.HandleMomoIpn(payload);

        result.Should().BeOfType<BadRequestResult>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
        db.PaymentTransactions.Should().BeEmpty();
        CompletedEvents(publish).Should().BeEmpty();
    }

    private static string BuildUrlEncodedInput(IEnumerable<KeyValuePair<string, string>> pairs)
        => string.Join("&", pairs.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

    private static string MomoRawSignature(MomoIpnPayload payload)
        => $"accessKey={MomoAccessKey}" +
           $"&amount={payload.Amount}" +
           $"&extraData={payload.ExtraData}" +
           $"&message={payload.Message}" +
           $"&orderId={payload.OrderId}" +
           $"&orderInfo={payload.OrderInfo}" +
           $"&orderType={payload.OrderType}" +
           $"&partnerCode={payload.PartnerCode}" +
           $"&payType={payload.PayType}" +
           $"&requestId={payload.RequestId}" +
           $"&responseTime={payload.ResponseTime}" +
           $"&resultCode={payload.ResultCode}" +
           $"&transId={payload.TransId}";

    private static string HmacSha512(string key, string input)
    {
        var builder = new StringBuilder();
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        foreach (var @byte in hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
            builder.Append(@byte.ToString("x2"));
        return builder.ToString();
    }

    private static string HmacSha256(string key, string input)
    {
        var builder = new StringBuilder();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        foreach (var @byte in hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
            builder.Append(@byte.ToString("x2"));
        return builder.ToString();
    }
}
