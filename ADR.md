# Architecture Decision Records

## ADR-001
Account chỉ thêm UID.
Tên và Server được lấy tự động từ game.

## ADR-002
Gold Block là Job độc lập, không chạy theo lịch Daily.

## ADR-003
GiftCode chỉ gồm:
- Code
- IsActive

## ADR-004
GiftCode dùng bool IsActive.

## ADR-005
Job chỉ là Queue, không lưu History.

## ADR-006
Chỉ Worker được phép gọi Playwright.
gg

## ADR-007
Một UID chỉ có một Automation Session tại một thời điểm.

## ADR-008
Repository không chứa Business Logic.

## ADR-009
Repository không dùng Generic Repository.

## ADR-010
Discord sử dụng Slash Command.
✅ ADR-011 - Add Account Workflow
Use Case: Add Account
Input
UID
Process
1. Kiểm tra UID đã tồn tại trong Database.

    Có
        ↓
    Trả lỗi "Account already exists."

    Không
        ↓

2. Gọi IPlayerClient.GetPlayerProfileAsync(uid).

    Không lấy được thông tin
        ↓
    Trả lỗi "Player not found."

    Thành công
        ↓

3. Tạo Account Entity.

        Uid
        Name
        Server

        ↓

4. Lưu Account vào Database.

        ↓

5. Kiểm tra DailyHistory.

    Hôm nay đã chạy?
        ↓
    Có
        ↓
    Bỏ qua

    Chưa
        ↓
    QueueService.CreateDailyJob()

        ↓

6. Lấy toàn bộ GiftCode đang Active.

        ↓

7. QueueService.CreateGiftJobs()

        ↓

8. Ghi Log

        ↓

9. Success
Output

Ví dụ Discord sẽ nhận:

✅ Account added successfully.

UID: 123456789
Name: Mạnh
Server: S123

Created:
• 1 Daily Job
• 18 Gift Jobs

Hoặc

❌ Account already exists.

Hoặc

❌ Cannot retrieve player information.

(ADR-012)

Từ bây giờ, mỗi Entity sẽ phải trả lời được một câu hỏi:

Account → "Quản lý tài khoản nào?"
Job → "Worker cần làm gì?"
GiftCode → "Code nào còn hiệu lực?"
GiftHistory → "Tài khoản này đã nhận code này chưa?"
DailyHistory → "Hôm nay tài khoản đã Daily chưa?"

Nếu một thuộc tính không giúp trả lời câu hỏi của Entity, thì chúng ta sẽ không thêm nó. Mình muốn dùng quy tắc này để giữ Domain luôn gọn và chỉ chứa những gì phục vụ trực tiếp cho nghiệp vụ.
ADR-013

Repository không được gọi Repository khác.

Ví dụ.

❌ Sai:

AccountRepository

↓

GiftRepository

Không.

Repository chỉ nói chuyện với Database.

Repository không biết Repository khác.

Service mới là nơi kết hợp chúng.
ADR-014

Job chỉ là Queue.

Không phải History.

ADR-015

Không Retry tự động.

Worker chạy một lần.

ADR-016

Mọi kết quả được lưu vào History.

Gift → GiftHistory.
Daily → DailyHistory.
ADR-017

Job sẽ bị xóa ngay sau khi Worker xử lý xong, bất kể thành công hay thất bại.