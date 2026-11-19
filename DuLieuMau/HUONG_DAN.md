# Nhập 18 mẫu xe một lần — MotoShopABG

Không cần tạo project mới, không cần thay code C#. Bộ này dùng đúng database của project MotoShopWeb đã gửi.

## 1. Chép ảnh (làm trước khi chạy SQL)
1. Giải nén file ZIP này.
2. Trong Visual Studio đang mở đúng web, nhấp phải project MotoShopWeb → Open Folder in File Explorer.
3. Mở wwwroot → images trong thư mục project. Nếu chưa có images thì tạo thư mục này.
4. Trong bộ giải nén, mở wwwroot → images, COPY nguyên thư mục catalog-abg vào wwwroot/images của project.
5. Kết quả đúng: thư mục chứa MotoShopWeb.csproj có đường dẫn con wwwroot/images/catalog-abg/cb650r-2026-1.png.
6. Không thay thế cả wwwroot; chỉ thêm thư mục catalog-abg.

## 2. Nhập dữ liệu một lần
1. Mở SQL Server Management Studio và kết nối đúng SQL Server mà web đang dùng.
2. Trong Object Explorer → Databases, bấm vào database của web hiện tại (trước đây là WebBanXeMoto; dùng tên thật trong ConnectionStrings:DefaultConnection của project).
3. File → Open → File → mở 01_NHAP_18_MAU_XE.sql trong bộ vừa giải nén.
4. Kiểm tra ô chọn database trên thanh công cụ, đổi từ master sang database web. Không chọn MotoShopABGDb nếu web vẫn dùng WebBanXeMoto.
5. Nhấn F5/Execute để chạy TOÀN BỘ file, không chỉ bôi đen một đoạn.
6. Results đầu tiên ghi DatabaseDaNhap và SoMauVuaThem. Nếu chưa từng nhập, số mẫu thêm là 18.
7. Nếu có mẫu trùng chính xác tên và hãng, script bỏ qua, hiển thị ở bảng kết quả cuối. Chạy lại không nhân đôi.
8. Lỗi bất kỳ trong lúc nhập: transaction rollback toàn bộ thay đổi của lần chạy. Chụp lỗi gửi để sửa, không xóa bảng.

## 3. Xem trên web
1. Chạy web bằng profile https nếu đang tắt.
2. Mở https://localhost:7085/Home/Vehicles rồi Ctrl+F5 để tải mới trình duyệt.
3. Danh sách có thêm 18 mẫu, phân trang tự hoạt động. Trang chủ hiện 6 mẫu mới nhất theo logic hiện có.
4. Thử tìm Honda, Kawasaki, Rebel hoặc Adventure; mở chi tiết để xem ảnh, công suất, hộp số, màu và mô tả.
5. Nếu ảnh không hiện: mở https://localhost:7085/images/catalog-abg/cb650r-2026-1.png. Nếu 404, thư mục ảnh đang chép sai nơi; phải chép vào đúng project đang chạy, không phải vào bin hoặc thư mục ZIP.
6. Admin vẫn sửa giá, thông tin hoặc ẩn các mẫu này bình thường.

## Dữ liệu được thêm
- 18 mẫu: 15 Honda, 3 Kawasaki; 7 nhóm kiểu dáng.
- Ảnh thật tải sẵn từ nguồn hãng, không cần trình duyệt tải ảnh từ website khác.
- Nhập vào HangXe/LoaiXe nếu tên chưa có, Xe, MauXe và AnhXe.
- Chỉ thêm ảnh/màu cho xe vừa tạo. Xe cũ trùng tên được bỏ qua hoàn toàn, không tự ghi đè giá hoặc trạng thái ẩn.
- Không tạo bảng mới, tài khoản, đơn hàng, giao dịch, xe thực tế, số khung hoặc số máy. Đây là danh mục để trưng bày, không phải xác nhận hàng sẵn trong kho.
- Giá đề xuất hãng dùng để tham khảo/demo. Đọc NGUON_DU_LIEU.md để đối chiếu giá, phiên bản và nguồn ảnh.
- Một số màu Kawasaki không được điền nếu phần dữ liệu ảnh của trang hãng không cung cấp nhãn rõ ràng.
- Năm trong tên là đời mẫu của hãng. Trường NamSanXuat để trống vì chưa có thông tin năm sản xuất xe thực tế.
- Bộ chống trùng dựa trên hãng + tên đầy đủ. Nếu bạn đã nhập cùng mẫu dưới tên khác, hãy dùng Admin kiểm tra và ẩn mẫu không cần trưng bày.

## Kiểm tra đã thực hiện
Đã kiểm tra tải ảnh thành công, định dạng ảnh, độ dài dữ liệu theo model hiện tại, tính đầy đủ 18 mẫu và cấu trúc ZIP. Đã rà soát SQL không UPDATE/DELETE, chỉ thêm quan hệ cho xe mới, dùng transaction và kiểm tra tồn tại để chạy lại không trùng.
Chưa thực thi SQL trên database của bạn vì không có quyền truy cập SQL Server đó. Cần kiểm tra kết quả F5 theo mục 2.
