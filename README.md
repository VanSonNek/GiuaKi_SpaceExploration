# Space Exploration — Đồ án giữa kỳ

Đồ án môn Lập trình Game bằng Unity. Repository lưu bản sao mã nguồn hiện tại của game bắn tàu không gian 2D theo chiều dọc.

## Môi trường

- Unity Editor **6000.5.10f1** (xem `ProjectSettings/ProjectVersion.txt`).
- Các dependency được khai báo trong `Packages/manifest.json` và `Packages/packages-lock.json`.
- Cần kết nối mạng khi Unity tải các package chưa có trên máy.

## Cách mở và chạy

1. Clone repository và thêm thư mục gốc vào Unity Hub bằng **Add project from disk**.
2. Mở bằng Unity **6000.5.10f1**, chờ import asset và resolve package hoàn tất.
3. Mở **`Assets/Scenes/GameScene.unity`** trong cửa sổ Project.
4. Nhấn **Play** trong Unity Editor để chạy game.

**Lưu ý:** Build Settings hiện chỉ chứa `SampleScene`. Khi muốn tạo bản build gameplay, cần chủ động thêm `GameScene` vào danh sách Scene của Build Settings/Build Profiles. Repository này giữ nguyên cấu hình hiện tại.

## Điều khiển

- **WASD hoặc các phím mũi tên:** di chuyển tàu.
- **Giữ Space:** bắn liên tục.
- **Escape hoặc nút Pause:** tạm dừng/tiếp tục.

## Các hệ thống hiện có trong mã nguồn

- Player di chuyển trong giới hạn màn hình, bắn và nhận sát thương.
- Enemy và thiên thạch xuất hiện theo từng đợt; thiên thạch có scale ngẫu nhiên và xoay.
- Boss xuất hiện sau các đợt, di chuyển ngang và bắn xuống.
- HP Player/Boss, điểm số, Pause/Resume, Victory/Game Over.
- Hai background cuộn liên tục và hiệu ứng nháy màu khi nhận sát thương.

Luồng Level 1: 4 Enemy → 6 Asteroid → 6 Enemy cùng 3 Asteroid → Boss. Các wave kết thúc khi đối tượng đã bị hủy, kể cả khi đi khỏi màn hình.

## Cấu trúc

- `Assets/Scripts/`: 23 script C#, chia theo các hệ thống gameplay.
- `Assets/Scenes/`: Scene gameplay và Scene mẫu.
- `Assets/Prefabs/`: Enemy, Asteroid, Boss và hai loại đạn.
- `Assets/Sprites/`: sprite sheet cho tàu, đạn, nền và UI.
- `Assets/Settings/`: cấu hình rendering, input và Scene Template.
- `Assets/TextMesh Pro/`: tài nguyên font, material và shader của TMP.
- `Packages/`, `ProjectSettings/`: dependency và cấu hình để khôi phục project.

Giữ nguyên toàn bộ file `.meta` khi sao chép hoặc làm việc với asset. `Library`, `Temp`, `Logs`, `UserSettings`, build output và file IDE tự sinh được loại khỏi Git.

## Trạng thái bản sao lưu

Đây là bản sao lưu mã nguồn đang phát triển; quy trình đưa lên Git không chạy Play Mode, build hoặc kiểm thử trên thiết bị.

Các điểm đã ghi nhận và chưa chỉnh sửa trong lần sao lưu này:

- URP trong manifest là `17.6.0`, trong lockfile/package cache hiện tại là `17.5.0`; cần xác minh package resolution trên máy mở mới.
- Một số tham chiếu tài nguyên trong Renderer2D, Scene Template và TMP Settings chưa đối chiếu được trên đĩa; cần kiểm tra trong Editor.
- Chưa có luồng Restart/Retry hoặc Main Menu trong mã nguồn đã rà soát.

## Tài nguyên bên thứ ba

Giấy phép font Liberation Sans nằm tại `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. Cần xác nhận điều kiện phân phối của sprite pack và các tài nguyên bên thứ ba trước khi công khai hoặc phát hành game.
