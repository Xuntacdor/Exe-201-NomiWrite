import { PenLine, MapPin } from "lucide-react";

const footerLinks = {
  "Sản phẩm": [
    "Cách hoạt động",
    "Tính năng",
    "Bảng giá",
    "Lộ trình phát triển",
  ],
  "Hỗ trợ": [
    "Câu hỏi thường gặp",
    "Liên hệ",
    "Chính sách bảo mật",
    "Điều khoản sử dụng",
  ],
  "Loại văn bản": [
    "IELTS Writing Task 2",
    "IELTS Writing Task 1",
    "Email công việc",
    "Luận học bổng",
  ],
};

export default function Footer() {
  return (
    <footer className="bg-slate-900 border-t border-slate-800">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 py-14">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10">
          {/* Brand */}
          <div className="md:col-span-1">
            <a href="#" className="flex items-center gap-2 mb-4">
              <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center">
                <PenLine className="w-4 h-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-base font-bold text-white">NomiWrite</span>
            </a>
            <p className="text-sm text-slate-400 leading-relaxed">
              Luyện viết tiếng Anh đúng cách — dạy thật, không sửa hộ.
              <br />
              Bài viết là điểm xuất phát, không phải đích đến.
            </p>
            <p className="mt-5 flex items-center gap-1.5 text-xs text-slate-600">
              <MapPin className="w-3 h-3 text-slate-500" />
              Made in Vietnam
            </p>
          </div>

          {/* Links */}
          {Object.entries(footerLinks).map(([category, links]) => (
            <div key={category}>
              <h4 className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-4">
                {category}
              </h4>
              <ul className="space-y-2.5">
                {links.map((link) => (
                  <li key={link}>
                    <a
                      href="#"
                      className="text-sm text-slate-500 hover:text-slate-300 transition-colors"
                    >
                      {link}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom bar */}
        <div className="mt-12 pt-6 border-t border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-3">
          <p className="text-xs text-slate-600">
            © 2026 NomiWrite. Bản quyền thuộc về NomiWrite.
          </p>
          <p className="text-xs text-slate-600">
            Điểm AI chấm chỉ mang tính tham khảo, không thay thế người chấm thật.
          </p>
        </div>
      </div>
    </footer>
  );
}
