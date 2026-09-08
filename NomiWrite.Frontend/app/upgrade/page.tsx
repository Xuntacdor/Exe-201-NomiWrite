"use client";

import { FormEvent, type ElementType, useState } from "react";
import Link from "next/link";
import Navbar from "../components/Navbar";
import Footer from "../components/Footer";
import {
  Check,
  Zap,
  CreditCard,
  Building2,
  Wallet,
  Lock,
  ChevronRight,
  ArrowLeft,
  ShieldCheck,
  Sparkles,
  BadgeCheck,
  Star,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { CheckoutRequest } from "@/lib/types";

const proFeatures = [
  "Không giới hạn số bài viết mỗi tháng",
  "Chấm điểm chi tiết theo từng tiêu chí",
  "Phân tích sâu tiến bộ theo thời gian",
  "Quiz ưu tiên nhóm lỗi yếu nhất của bạn",
  "Sổ từ vựng cá nhân đầy đủ",
  "Lưu lịch sử bài viết không giới hạn",
  "Xuất báo cáo tiến bộ PDF",
  "Hỗ trợ ưu tiên qua email",
];

const testimonials = [
  { name: "Minh Châu", role: "Sinh viên luyện IELTS", text: "Sau 3 tháng dùng NomiWrite, band writing của mình tăng từ 5.5 lên 7.0." },
  { name: "Hoàng Việt", role: "Đi làm, luyện viết email", text: "Quiz sinh tự động từ đúng lỗi của mình — cái này là điểm khác biệt thật sự." },
];

type PaymentMethod = "card" | "bank" | "momo";

const paymentMethods: { id: PaymentMethod; label: string; icon: ElementType; sub: string }[] = [
  { id: "card", label: "Thẻ tín dụng / ghi nợ", icon: CreditCard, sub: "Visa · Mastercard · JCB" },
  { id: "bank", label: "Chuyển khoản ngân hàng", icon: Building2,  sub: "Thanh toán qua QR Code" },
  { id: "momo", label: "Ví MoMo",                icon: Wallet,     sub: "Thanh toán nhanh qua app" },
];

function fmt(n: number) {
  return n.toLocaleString("vi-VN") + "đ";
}

export default function UpgradePage() {
  const [method, setMethod]   = useState<PaymentMethod>("card");
  const [billing, setBilling] = useState<"monthly" | "yearly">("monthly");
  const [agreed, setAgreed]   = useState(false);
  const [loading, setLoading] = useState(false);
  const [done, setDone]       = useState(false);
  const [error, setError]     = useState("");

  const monthlyPrice = 199_000;
  const yearlyPrice  = 159_000;
  const price        = billing === "monthly" ? monthlyPrice : yearlyPrice;
  const total        = billing === "monthly" ? monthlyPrice : yearlyPrice * 12;
  const saving       = monthlyPrice * 12 - total;

  const paymentMethodByUi: Record<PaymentMethod, CheckoutRequest["paymentMethod"]> = {
    card: "vnpay",
    bank: "vietqr",
    momo: "momo",
  };

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!agreed) return;
    setError("");

    if (!getSession()?.accessToken) {
      window.location.href = "/login";
      return;
    }

    try {
      setLoading(true);
      const checkout = await apiClient.createCheckout({
        plan: "premium",
        billingCycle: billing,
        paymentMethod: paymentMethodByUi[method],
        amount: total,
        currency: "VND",
      });

      if (checkout.checkoutUrl) {
        window.location.assign(checkout.checkoutUrl);
        return;
      }

      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Khong the tao thanh toan. Vui long thu lai.");
    } finally {
      setLoading(false);
    }
  }

  /* ── Success screen ── */
  if (done) {
    return (
      <>
        <Navbar />
        <main className="min-h-screen bg-slate-50 flex items-center justify-center px-4 pt-16">
          <div className="bg-white rounded-3xl border border-slate-100 shadow-xl max-w-md w-full p-10 text-center my-16">
            <div className="w-20 h-20 rounded-full bg-gradient-to-br from-blue-500 to-violet-600 flex items-center justify-center mx-auto mb-6 shadow-xl shadow-blue-200">
              <BadgeCheck className="w-10 h-10 text-white" />
            </div>
            <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-50 border border-emerald-100 mb-4">
              <span className="text-xs font-bold text-emerald-700">Kích hoạt thành công</span>
            </div>
            <h2 className="text-2xl font-extrabold text-slate-900 mb-3">
              Chào mừng bạn đến với Premium!
            </h2>
            <p className="text-sm text-slate-500 leading-relaxed mb-8">
              Tài khoản đã được nâng cấp lên{" "}
              <span className="font-bold text-blue-600">NomiWrite Premium</span>.
              Viết không giới hạn, theo dõi tiến bộ và đạt mục tiêu nhanh hơn.
            </p>
            <a
              href="/dashboard"
              className="flex items-center justify-center gap-2 w-full py-4 rounded-2xl bg-gradient-to-r from-blue-600 to-violet-600 text-white text-sm font-extrabold hover:opacity-90 transition-all shadow-lg shadow-blue-200"
            >
              Vào Dashboard <ChevronRight className="w-4 h-4" />
            </a>
            <a href="/write" className="block mt-3 text-sm text-blue-500 hover:text-blue-700 font-semibold transition-colors">
              Viết bài đầu tiên ngay →
            </a>
          </div>
        </main>
        <Footer />
      </>
    );
  }

  return (
    <>
      <Navbar />

      <main className="min-h-screen bg-slate-50 pt-16">

        {/* Hero strip */}
        <div className="relative bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700 overflow-hidden">
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(255,255,255,0.08),transparent_60%)]" />
          <div className="relative max-w-5xl mx-auto px-4 sm:px-6 py-12 text-center">
            <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-white/10 border border-white/20 mb-4">
              <Zap className="w-3.5 h-3.5 text-amber-300 fill-amber-300" />
              <span className="text-xs font-bold text-white">Nâng cấp lên Premium</span>
            </div>
            <h1 className="text-3xl sm:text-4xl font-extrabold text-white mb-3">
              Học không giới hạn,{" "}
              <br className="sm:hidden" />
              tiến bộ rõ ràng
            </h1>
            <p className="text-blue-100 text-sm max-w-md mx-auto">
              Bắt đầu miễn phí rồi nâng cấp khi bạn thấy giá trị thật.
              Hủy bất cứ lúc nào, hoàn tiền trong 7 ngày.
            </p>
          </div>
        </div>

        <div className="max-w-5xl mx-auto px-4 sm:px-6 py-12">
          {/* Back link */}
          <Link
            href="/#pricing"
            className="inline-flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-800 transition-colors font-medium mb-8"
          >
            <ArrowLeft className="w-4 h-4" />
            Xem lại bảng giá
          </Link>

          <div className="grid grid-cols-1 lg:grid-cols-5 gap-8">

            {/* ── LEFT col ── */}
            <div className="lg:col-span-2 space-y-5">

              {/* Billing toggle */}
              <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3">
                  Chu kỳ thanh toán
                </p>
                <div className="flex gap-2">
                  <button
                    onClick={() => setBilling("monthly")}
                    className={`flex-1 py-3 rounded-xl text-sm font-bold transition-all ${
                      billing === "monthly"
                        ? "bg-blue-600 text-white shadow-md shadow-blue-200"
                        : "bg-slate-100 text-slate-500 hover:bg-slate-200"
                    }`}
                  >
                    Hàng tháng
                  </button>
                  <button
                    onClick={() => setBilling("yearly")}
                    className={`flex-1 py-3 rounded-xl text-sm font-bold transition-all relative ${
                      billing === "yearly"
                        ? "bg-blue-600 text-white shadow-md shadow-blue-200"
                        : "bg-slate-100 text-slate-500 hover:bg-slate-200"
                    }`}
                  >
                    Hàng năm
                    <span className="absolute -top-2.5 -right-1 px-1.5 py-0.5 bg-amber-400 text-amber-900 text-[10px] font-extrabold rounded-full">
                      -20%
                    </span>
                  </button>
                </div>
                {billing === "yearly" && (
                  <p className="mt-3 text-center text-xs text-emerald-700 bg-emerald-50 rounded-xl py-2 font-semibold">
                    Tiết kiệm {fmt(saving)} so với tháng lẻ 🎉
                  </p>
                )}
              </div>

              {/* Plan card */}
              <div className="relative rounded-2xl overflow-hidden shadow-md">
                <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700" />
                <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_left,rgba(255,255,255,0.1),transparent_60%)]" />
                <div className="relative p-6">
                  <div className="flex justify-end mb-3">
                    <div className="inline-flex items-center gap-1 px-2.5 py-1 bg-amber-400 text-amber-900 text-[10px] font-extrabold rounded-full">
                      <Sparkles className="w-3 h-3" />
                      Phổ biến nhất
                    </div>
                  </div>

                  <p className="text-xs font-bold text-blue-200 uppercase tracking-widest mb-1">Premium</p>
                  <div className="flex items-end gap-1 mb-1">
                    <span className="text-4xl font-extrabold text-white">{fmt(price)}</span>
                    <span className="text-xs text-blue-200 mb-1.5">/ tháng</span>
                  </div>
                  {billing === "yearly" && (
                    <p className="text-xs text-blue-200 mb-4">
                      Thanh toán{" "}
                      <span className="text-white font-bold">{fmt(total)}</span> / năm
                    </p>
                  )}

                  <div className="mt-4 space-y-2.5">
                    {proFeatures.map((f) => (
                      <div key={f} className="flex items-start gap-2.5">
                        <div
                          className="rounded-full bg-white/20 flex items-center justify-center shrink-0 mt-0.5"
                          style={{ width: 17, height: 17 }}
                        >
                          <Check className="w-2.5 h-2.5 text-white" />
                        </div>
                        <span className="text-xs text-blue-50 leading-relaxed">{f}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              {/* Testimonials */}
              <div className="space-y-3">
                {testimonials.map((t) => (
                  <div key={t.name} className="bg-white rounded-2xl border border-slate-100 p-4 shadow-sm">
                    <div className="flex items-center gap-1 mb-2">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <Star key={i} className="w-3 h-3 fill-amber-400 text-amber-400" />
                      ))}
                    </div>
                    <p className="text-xs text-slate-600 leading-relaxed italic mb-3">&quot;{t.text}&quot;</p>
                    <p className="text-xs font-bold text-slate-800">{t.name}</p>
                    <p className="text-[11px] text-slate-400">{t.role}</p>
                  </div>
                ))}
              </div>

              {/* Trust */}
              <div className="bg-white rounded-2xl border border-slate-100 p-4 shadow-sm space-y-3">
                {[
                  { icon: ShieldCheck, text: "Thanh toán bảo mật SSL 256-bit" },
                  { icon: Lock,        text: "Không lưu thông tin thẻ trên server" },
                  { icon: BadgeCheck,  text: "Hoàn tiền trong 7 ngày nếu không hài lòng" },
                ].map(({ icon: Icon, text }) => (
                  <div key={text} className="flex items-center gap-3">
                    <Icon className="w-4 h-4 text-emerald-500 shrink-0" />
                    <span className="text-xs text-slate-500">{text}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* ── RIGHT col: form ── */}
            <form onSubmit={handleSubmit} className="lg:col-span-3 space-y-5">

              {/* Payment method */}
              <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-4">
                  Phương thức thanh toán
                </p>
                <div className="space-y-2">
                  {paymentMethods.map(({ id, label, icon: Icon, sub }) => (
                    <label
                      key={id}
                      className={`flex items-center gap-4 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                        method === id
                          ? "border-blue-500 bg-blue-50/60"
                          : "border-slate-100 hover:border-slate-200"
                      }`}
                    >
                      <input
                        type="radio" name="method" value={id}
                        checked={method === id} onChange={() => setMethod(id)}
                        className="sr-only"
                      />
                      <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${
                        method === id ? "bg-blue-100" : "bg-slate-100"
                      }`}>
                        <Icon className={`w-5 h-5 ${method === id ? "text-blue-600" : "text-slate-400"}`} />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className={`text-sm font-bold ${method === id ? "text-blue-800" : "text-slate-700"}`}>
                          {label}
                        </p>
                        <p className="text-xs text-slate-400 mt-0.5">{sub}</p>
                      </div>
                      <div className={`w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0 transition-all ${
                        method === id ? "border-blue-500 bg-blue-500" : "border-slate-300"
                      }`}>
                        {method === id && <div className="w-2 h-2 rounded-full bg-white" />}
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              {/* Card form */}
              {method === "card" && (
                <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm space-y-4">
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-wider">Thông tin thẻ</p>

                  <div>
                    <label className="block text-xs font-semibold text-slate-600 mb-1.5">Tên chủ thẻ</label>
                    <input
                      type="text" placeholder="NGUYEN VAN A"
                      className="w-full px-4 py-3.5 rounded-xl border-2 border-slate-200 text-sm font-medium text-slate-800 placeholder:text-slate-300 focus:outline-none focus:border-blue-400 transition-colors uppercase tracking-wide"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-600 mb-1.5">Số thẻ</label>
                    <div className="relative">
                      <input
                        type="text" placeholder="1234  5678  9012  3456" maxLength={19}
                        className="w-full px-4 py-3.5 rounded-xl border-2 border-slate-200 text-sm font-medium text-slate-800 placeholder:text-slate-300 focus:outline-none focus:border-blue-400 transition-colors tracking-widest pr-12"
                      />
                      <CreditCard className="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-semibold text-slate-600 mb-1.5">Ngày hết hạn</label>
                      <input
                        type="text" placeholder="MM / YY" maxLength={7}
                        className="w-full px-4 py-3.5 rounded-xl border-2 border-slate-200 text-sm font-medium text-slate-800 placeholder:text-slate-300 focus:outline-none focus:border-blue-400 transition-colors"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-slate-600 mb-1.5">CVV</label>
                      <div className="relative">
                        <input
                          type="password" placeholder="•••" maxLength={4}
                          className="w-full px-4 py-3.5 rounded-xl border-2 border-slate-200 text-sm font-medium text-slate-800 placeholder:text-slate-300 focus:outline-none focus:border-blue-400 transition-colors"
                        />
                        <Lock className="absolute right-4 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-300" />
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* Bank transfer */}
              {method === "bank" && (
                <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-5">
                    Thông tin chuyển khoản
                  </p>
                  <div className="flex flex-col items-center gap-5">
                    <div className="w-44 h-44 rounded-2xl bg-slate-100 border-2 border-dashed border-slate-200 flex flex-col items-center justify-center gap-2">
                      <Building2 className="w-10 h-10 text-slate-300" />
                      <span className="text-xs text-slate-400 text-center px-6">
                        QR hiển thị sau khi xác nhận
                      </span>
                    </div>
                    <div className="w-full divide-y divide-slate-50">
                      {[
                        { label: "Ngân hàng",     value: "Vietcombank" },
                        { label: "Số tài khoản",  value: "1234 5678 90" },
                        { label: "Chủ tài khoản", value: "CONG TY NOMIWRITE" },
                        { label: "Nội dung CK",   value: "PREMIUM MQ070626" },
                        { label: "Số tiền",        value: fmt(total) },
                      ].map(({ label, value }) => (
                        <div key={label} className="flex justify-between items-center py-2.5">
                          <span className="text-xs text-slate-400">{label}</span>
                          <span className="text-xs font-bold text-slate-800">{value}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              )}

              {/* MoMo */}
              {method === "momo" && (
                <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm text-center">
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-6">
                    Thanh toán qua MoMo
                  </p>
                  <div className="w-24 h-24 rounded-3xl bg-pink-50 flex items-center justify-center mx-auto mb-5">
                    <Wallet className="w-12 h-12 text-pink-500" />
                  </div>
                  <p className="text-sm text-slate-600 leading-relaxed">
                    Nhấn{" "}
                    <span className="font-bold text-pink-600">Thanh toán ngay</span>{" "}
                    để được chuyển hướng đến ứng dụng MoMo.
                  </p>
                  <p className="mt-2 text-xs text-slate-400">
                    Số tiền:{" "}
                    <span className="font-bold text-slate-700">{fmt(total)}</span>
                    {billing === "yearly" && " (1 lần / năm)"}
                  </p>
                </div>
              )}

              {/* Order summary */}
              <div className="bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
                <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-4">
                  Tóm tắt đơn hàng
                </p>
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-slate-500">
                      NomiWrite Premium —{" "}
                      {billing === "monthly" ? "1 tháng" : "12 tháng"}
                    </span>
                    <span className="font-bold text-slate-800">
                      {fmt(billing === "monthly" ? monthlyPrice : monthlyPrice * 12)}
                    </span>
                  </div>
                  {billing === "yearly" && (
                    <div className="flex justify-between text-sm">
                      <span className="text-slate-500">Giảm giá gói năm (-20%)</span>
                      <span className="font-bold text-emerald-600">-{fmt(saving)}</span>
                    </div>
                  )}
                  <div className="pt-3 border-t border-slate-100 flex justify-between items-center">
                    <span className="text-sm font-extrabold text-slate-900">Tổng cộng</span>
                    <div className="text-right">
                      <span className="text-xl font-extrabold text-blue-600">{fmt(total)}</span>
                      {billing === "yearly" && (
                        <p className="text-[10px] text-slate-400">thanh toán 1 lần / năm</p>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Agreement */}
              <label className="flex items-start gap-3 cursor-pointer group">
                <div
                  onClick={() => setAgreed(!agreed)}
                  className={`mt-0.5 w-5 h-5 rounded-md border-2 flex items-center justify-center shrink-0 transition-all ${
                    agreed
                      ? "bg-blue-600 border-blue-600"
                      : "border-slate-300 group-hover:border-slate-400"
                  }`}
                >
                  {agreed && <Check className="w-3 h-3 text-white" />}
                </div>
                <span className="text-xs text-slate-500 leading-relaxed">
                  Tôi đồng ý với{" "}
                  <a href="#" className="text-blue-500 hover:underline font-medium">
                    Điều khoản dịch vụ
                  </a>
                  {" "}và{" "}
                  <a href="#" className="text-blue-500 hover:underline font-medium">
                    Chính sách bảo mật
                  </a>
                  {" "}của NomiWrite. Tôi hiểu rằng gói sẽ tự gia hạn và có thể hủy bất cứ lúc nào.
                </span>
              </label>

              {error && (
                <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">
                  {error}
                </p>
              )}

              {/* Submit button */}
              <button
                type="submit"
                disabled={!agreed || loading}
                className={`w-full py-4 rounded-2xl text-sm font-extrabold flex items-center justify-center gap-2.5 transition-all ${
                  agreed && !loading
                    ? "bg-gradient-to-r from-blue-600 to-violet-600 text-white hover:from-blue-700 hover:to-violet-700 shadow-xl shadow-blue-200 hover:-translate-y-0.5 active:translate-y-0"
                    : "bg-slate-200 text-slate-400 cursor-not-allowed"
                }`}
              >
                {loading ? (
                  <>
                    <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                    Đang xử lý thanh toán...
                  </>
                ) : (
                  <>
                    <Lock className="w-4 h-4" />
                    Thanh toán ngay — {fmt(total)}
                  </>
                )}
              </button>

              <p className="text-center text-[11px] text-slate-400 flex items-center justify-center gap-1.5">
                <ShieldCheck className="w-3.5 h-3.5 text-emerald-400" />
                Kết nối mã hóa SSL · Thông tin của bạn được bảo mật tuyệt đối
              </p>
            </form>
          </div>
        </div>
      </main>

      <Footer />
    </>
  );
}
