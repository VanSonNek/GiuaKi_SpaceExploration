# Tra cứu code bảo vệ đồ án Space Exploration

Các câu trả lời dưới đây dựa trên 34 script C# hiện có trong `Assets/Scripts/`. Đây là tài liệu giải thích code, không phải kết quả kiểm thử gameplay mới.

**Cách tìm nhanh:** mở file bằng liên kết trong bảng rồi dùng **Ctrl+F** với từ khóa ở cột cuối. Dùng **Ctrl+Shift+F → `BAO VE`** để tìm các chú thích trong toàn project. Các nhãn mới có dạng `// [BAO VE - TEN CHUC NANG]`.

**Khi cô hỏi “quái được sinh ở đâu?”:** mở `EnemyFormationSpawner.Run()` và chỉ dòng `Instantiate(PrefabForSlot(...), spawn, ...)`. Với quái kiểu thường, dòng tương ứng ở `EnemySpawner.SpawnWave()`. `Level1Controller` quyết định thời điểm và cấu hình, không trực tiếp Instantiate Enemy.

**Luồng Enemy bị bắn:** `Bullet.OnTriggerEnter2D()` → `Enemy.TakeDamage()` → trừ HP → nếu hết HP: đánh dấu chết, cộng điểm, tạo hiệu ứng nếu có, phát `Defeated`, hủy Enemy. `PowerUpDropManager.OnEnemyDefeated()` chỉ xét xác suất rơi, không bảo đảm luôn có vật phẩm.

**Luồng Enemy đâm Player:** `Enemy.OnTriggerEnter2D()` → đặt `hasHitPlayer` → `PlayerHealth.TakeDamage(contactDamage)` → tạo hiệu ứng nếu có → hủy Enemy. Nhánh này không gọi `AddScore` hoặc `Defeated`, nên không cộng điểm và không kích hoạt xét rơi vật phẩm từ Enemy.

**Lưu ý thông số:** giá trị khai báo trong C# là mặc định; Scene/Prefab có thể lưu giá trị riêng trong Inspector. Khi hỏi số Wave, HP Boss hoặc xác suất đang dùng ở một Level cụ thể, cần đối chiếu Inspector tương ứng.

## Player và đạn Player

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
|---|---|---|---|---|
| Máy bay nhận phím di chuyển ở đâu? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `Update()`, `Input.GetAxisRaw` | Đọc hai trục Horizontal/Vertical; InputManager của project ánh xạ WASD và phím mũi tên. | `PLAYER NHAN PHIM` |
| Tốc độ di chuyển được tính thế nào? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `Update()`, `moveSpeed`, `Time.deltaTime` | Lấy hướng nhân tốc độ và thời gian của frame để tính quãng đường. | `movement.normalized` |
| Vì sao đi chéo không nhanh hơn đi thẳng? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `movement.normalized` | Chuẩn hóa vector hướng về độ dài 1 trước khi nhân tốc độ. | `movement.normalized` |
| Rẽ trái/phải đổi hình tàu ở đâu? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `ChangeShipSprite()` | Dấu của input ngang chọn leftSprite, rightSprite hoặc straightSprite. | `void ChangeShipSprite` |
| Máy bay không vượt màn hình nhờ đâu? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `LimitPlayerInsideScreen()`, `Mathf.Clamp` | Kẹp tọa độ X/Y theo Camera và chừa khoảng bằng nửa kích thước tàu. | `PLAYER CLAMP CAMERA` |
| Sprite bounds dùng làm gì? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `spriteRenderer.bounds.extents` | Lấy nửa kích thước Sprite trong thế giới, đã tính Scale, để giữ cả thân tàu trong màn hình. | `PLAYER SPRITE BOUNDS` |
| Nhấn Space bắn ở đâu? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `Update()`, `TryFire()` | Giữ Space sẽ gọi TryFire; hàm này kiểm tra điều kiện trước khi tạo đạn. | `Input.GetKey(KeyCode.Space)` |
| Tốc độ bắn nằm ở biến nào? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `fireRate`, `nextFireTime`, `TryFire()` | fireRate là số giây nghỉ giữa hai loạt; nextFireTime là mốc được bắn lại. | `PLAYER TOC DO BAN` |
| Vì sao Pause hoặc chết thì không bắn? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `TryFire()` | Guard kiểm tra timeScale, CurrentHealth, component, Prefab, FirePoint và cooldown. | `PLAYER DIEU KIEN BAN` |
| Chỉ đúng dòng tạo đạn Player? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `SpawnBullet()`, `Instantiate` | Instantiate tạo bản sao bulletPrefab tại position với rotation đã tính. | `SINH DAN PLAYER` |
| FirePoint có vai trò gì? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `TryFire()`, `firePoint.position` | Cung cấp gốc bắn trong thế giới; SpawnBullet còn điều chỉnh để đạn không nằm trong Collider Player. | `Vector3 origin = firePoint.position` |
| Súng cấp 1, 2, 3 khác nhau ở đâu? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `TryFire()`, `WeaponLevel` | Cấp 1 một tia giữa; cấp 2 hai tia ngoài song song; cấp 3 thêm tia giữa và xoay hai tia ngoài. | `PLAYER TIA GIUA` |
| Góc quạt cấp 3 nằm ở đâu? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `levelThreeSpreadAngle`, `TryFire()` | Hai tia ngoài dùng góc ngược dấu; cấu hình mặc định là 15 độ mỗi bên. | `float angle = WeaponLevel == 3` |
| Vì sao tia ngoài bay chếch thật? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `Update()`, `transform.up` | Đạn di chuyển theo trục up của chính nó, đã xoay lúc tạo, thay vì luôn dùng Vector3.up. | `transform.up * speed` |
| Đạn tự biến mất sau bao lâu được xử lý ở đâu? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `Start()`, `lifeTime` | Destroy có thời gian chờ giới hạn tuổi thọ viên đạn; va chạm mục tiêu cũng có thể hủy sớm. | `Destroy(gameObject, lifeTime)` |
| Cấp súng tăng và giới hạn tối đa ở đâu? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `UpgradeWeapon()`, `WeaponLevel` | Guard chặn từ cấp 3 trở lên, sau đó mới tăng WeaponLevel. | `PLAYER GIOI HAN CAP SUNG` |
| HUD biết súng vừa tăng cấp bằng cách nào? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `WeaponLevelChanged` | Sau khi tăng cấp, phát sự kiện mang cấp mới để WeaponLevelUI hiển thị. | `PLAYER SU KIEN CAP SUNG` |
| HP ban đầu của Player ở đâu? | [PlayerHealth.cs](Assets/Scripts/Player/PlayerHealth.cs) | `Start()`, `maxHealth` | Mỗi Player mới nhận currentHealth = maxHealth; mặc định maxHealth là 3. | `currentHealth = maxHealth` |
| Đâu là dòng trừ HP Player? | [PlayerHealth.cs](Assets/Scripts/Player/PlayerHealth.cs) | `TakeDamage()` | currentHealth -= damage, sau đó nháy màu và kiểm tra chết. | `PLAYER TRU HP` |
| Hồi máu vì sao không vượt tối đa? | [PlayerHealth.cs](Assets/Scripts/Player/PlayerHealth.cs) | `Heal()`, `Mathf.Min` | Chỉ hồi phần nhỏ hơn giữa lượng yêu cầu và HP còn thiếu; không hồi sinh Player đã chết. | `PLAYER HOI KHONG VUOT MAX` |
| Player chết thông báo cho ai? | [PlayerHealth.cs](Assets/Scripts/Player/PlayerHealth.cs) | `TakeDamage()`, `Died` | Đặt isDead rồi phát Died trước khi hủy Player; GameResultManager nghe để bật Game Over. | `PLAYER BAO GAME OVER` |
| Player có sự kiện HealthChanged không? | [PlayerHealth.cs](Assets/Scripts/Player/PlayerHealth.cs), [PlayerHealthUI.cs](Assets/Scripts/UI/PlayerHealthUI.cs) | `CurrentHealth`, `PlayerHealthUI.Update()` | Hiện không có HealthChanged; thanh HP đọc CurrentHealth mỗi frame. | `CAP NHAT HP UI` trong PlayerHealthUI |

## Enemy: trúng đạn, va Player, điểm và vật phẩm

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
| Hàm nào phát hiện đạn Player trúng Enemy? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `OnTriggerEnter2D()`, `GetComponent<Enemy>()` | Callback nằm trên đạn; tìm Enemy trên object có Collider vừa chạm rồi gọi TakeDamage. | `DAN PHAT HIEN ENEMY` |
| Đâu là lệnh gửi sát thương sang Enemy? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `enemy.TakeDamage(damage)` | Bullet gửi lượng damage; Enemy.cs mới là nơi trừ máu. | `DAN GAY DAMAGE ENEMY` |
| Đâu là dòng thực sự trừ HP Enemy? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `TakeDamage()`, `currentHealth -= damage` | Trừ sát thương được truyền vào khỏi HP hiện tại. | `ENEMY TRU HP` |
| Đâu là điều kiện xác định Enemy hết máu? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `TakeDamage()`, `currentHealth <= 0` | Chỉ nhánh HP không còn dương mới xử lý tiêu diệt. | `ENEMY CHET` |
| Vì sao không xử lý Enemy chết nhiều lần? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `isDead`, `hasHitPlayer` | Guard chặn Enemy đã chết/đâm Player; isDead được đặt trước khi cộng điểm và phát sự kiện. | `ENEMY DANH DAU CHET` |
| Enemy nổ khi bị bắn ở dòng nào? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `Instantiate(deathEffectPrefab, ...)` | Tạo hiệu ứng tại vị trí Enemy nếu Prefab hiệu ứng đã được gán. | `ENEMY HIEU UNG NO` |
| Đâu là dòng hủy Enemy đã hết HP? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `TakeDamage()`, `Destroy(gameObject)` | Hủy sau khi đã cộng điểm, tạo hiệu ứng và phát Defeated. | `ENEMY HUY SAU KHI CHET` |
| Enemy cộng điểm lúc nào? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `ScoreManager.Instance.AddScore(scoreValue)` | Chỉ trong nhánh Enemy hết HP do TakeDamage và khi ScoreManager tồn tại. | `ENEMY CONG DIEM` |
| Enemy tự tạo vật phẩm trong Enemy.cs phải không? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `Defeated?.Invoke(this)` | Không; Enemy phát Defeated, DropManager mới xét xác suất và sinh vật phẩm. | `ENEMY ROI VAT PHAM` |
| Enemy đâm Player được xử lý ở đâu? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `OnTriggerEnter2D()`, `hasHitPlayer` | Đánh dấu đã va Player trước khi gây sát thương, tạo hiệu ứng nếu có rồi hủy Enemy. | `ENEMY VA CHAM PLAYER` |
| Đâu là dòng Enemy gây contactDamage? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `playerHealth.TakeDamage(contactDamage)` | Gọi trực tiếp PlayerHealth; không phải trừ HP của Enemy. | `ENEMY CONTACT DAMAGE` |
| Vì sao Enemy đâm Player không gây damage hai lần? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `hasHitPlayer`, `isDead` | OnTriggerEnter2D kiểm tra cờ trước khi xử lý; lần đầu đặt hasHitPlayer ngay trước gây damage. | `ENEMY CHAN VA CHAM LAP` |
| Enemy đâm Player vì sao không cộng điểm hoặc rơi đồ? | [Enemy.cs](Assets/Scripts/Enemy/Enemy.cs) | `OnTriggerEnter2D()`, `Destroy(gameObject)` | Nhánh va Player không gọi AddScore và không phát Defeated. | `ENEMY HUY SAU VA CHAM` |
| Enemy ra khỏi màn hình có được tính là bắn hạ không? | [EnemyMovement.cs](Assets/Scripts/Enemy/EnemyMovement.cs) | `Update()`, `Destroy(gameObject)` | Không; tự hủy dưới Camera không gọi Enemy.TakeDamage nên không cộng điểm hoặc phát Defeated. | `spriteRenderer.bounds.max.y <= screenBottom` |
| Một viên đạn có gây damage nhiều mục tiêu trong frame không? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `hasHit`, `OnTriggerEnter2D()` | Có cờ hasHit chặn lần xử lý tiếp theo vì Destroy không xóa object ngay tại lệnh gọi. | `if (hasHit)` |

## Spawn, Movement và Wave

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
| Bắt đầu Level từ đâu? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `IEnumerator Start()`, `sequenceStarted` | Unity chạy Start dạng coroutine; cờ bảo đảm chuỗi chỉ khởi động một lần. | `IEnumerator Start` |
| Số Wave và cấu hình từng Wave nằm ở đâu? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `customWaves`, `WaveDefinition` | Mỗi Scene gán mảng Wave riêng; độ dài mảng là số Wave khi bật useCustomWaves. | `DU LIEU WAVE` |
| Bắt đầu Wave tiếp theo ở đâu? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `RunCustomWaves()`, `currentCustomWaveIndex` | Vòng for lấy cấu hình, kiểm tra điều kiện rồi cập nhật chỉ số Wave. | `BAT DAU WAVE` |
| Số Enemy mỗi lượt do đâu quyết định? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs), [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `WaveDefinition.enemyCount`, `EnemyFormationSettings.enemyCount` | Spawn thường dùng enemyCount của Wave; có Formation thì dùng số Enemy trong từng nhóm cấu hình. | `DU LIEU WAVE` trong Level1Controller |
| Chỉ đúng dòng sinh quái kiểu thường? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs) | `SpawnWave()`, `Instantiate` | Tạo bản sao enemyPrefab; controller chỉ gọi StartEnemyWave để yêu cầu sinh. | `SINH QUAI THUONG` |
| Chỉ đúng dòng sinh quái đội hình? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `Run()`, `Instantiate` | Tạo Enemy tại spawn từ mẫu do PrefabForSlot chọn. | `SINH QUAI FORMATION` |
| Tọa độ Enemy thường được chọn thế nào? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs) | `SpawnWave()`, `Random.Range` | X ngẫu nhiên trong bề ngang Camera có tính thân tàu; cạnh dưới Enemy đặt trên màn hình. | `TOA DO SINH QUAI` |
| Enemy nhiều hình dáng được chọn ở đâu? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `PrefabForSlot()`, `visualVariants` | Luân phiên mẫu gốc và biến thể; cặp cánh/hộ tống dùng biến thể đối xứng. | `static GameObject PrefabForSlot` |
| V, hai cột và đội hình khác được tính ở đâu? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `BuildSlots()`, `formationType` | switch chọn thuật toán tạo tọa độ slot; chưa tạo GameObject ở đây. | `CHON KIEU FORMATION` |
| SideAttack có thật sự đi từ hai bên không? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `Run()`, `spawnDirection` | Điểm spawn nằm ngoài cạnh trái/phải, sau đó Member đưa tàu tới đích trong màn hình. | `SIDE ATTACK DIEM VAO` |
| HoldAndShoot giữ rồi rời vị trí thế nào? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs), [EnemyFormationMember.cs](Assets/Scripts/Enemy/EnemyFormationMember.cs) | `Run()`, `Release()`, `holdDuration` | Chờ vào đích, giữ một thời gian rồi Release; Member chỉ thay chuyển động, không tắt EnemyShooting. | `FORMATION GIU VA THA` trong Spawner |
| Enemy bay thẳng, Zigzag, Diagonal ở đâu? | [EnemyMovement.cs](Assets/Scripts/Enemy/EnemyMovement.cs) | `Update()`, `MoveHorizontally()` | Cùng bay xuống; Zigzag cộng dao động Sin, Diagonal cộng dịch ngang và đảo hướng tại biên. | `ENEMY ZIGZAG` |
| Enemy nào được bắn và bắn ở đâu? | [EnemyShooting.cs](Assets/Scripts/Enemy/EnemyShooting.cs) | `Start()`, `IsShooter`, `Update()` | Chọn xác suất một lần, sau đó bắn theo cooldown khi tàu nằm đủ trong Camera. | `SINH DAN ENEMY` |
| Khi nào Wave được tính hoàn tất? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs), [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs) | `AllEnemiesDestroyed()`, `IsWaveComplete` | Phải sinh đủ và chờ các Enemy được theo dõi biến mất; sinh xong chưa đủ để chuyển Wave. | `KET THUC WAVE ENEMY` trong EnemySpawner |
| Controller chuyển Wave sau điều kiện nào? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `RunCustomWaves()`, `RunFormationGroups()` | Chờ các phần đang dùng hoàn tất hợp lệ, nghỉ delayAfterWave rồi vòng for chuyển sang Wave tiếp. | `CHUYEN WAVE TIEP THEO` |
| Thiên thạch được sinh ở đâu? | [AsteroidSpawner.cs](Assets/Scripts/Asteroid/AsteroidSpawner.cs) | `SpawnWave()`, `SpawnCorridor()` | Hai coroutine dùng Instantiate với cùng asteroidPrefab nhưng chọn vị trí theo cách khác nhau. | `SINH ASTEROID` |
| Thiên thạch gây sát thương và cho điểm thế nào? | [Asteroid.cs](Assets/Scripts/Asteroid/Asteroid.cs) | `TakeDamage()`, `OnTriggerEnter2D()` | Bắn vỡ mới cộng điểm; chạm Player gây contactDamage rồi hủy, không phát Enemy.Defeated. | `ASTEROID CONG DIEM` |
| Wave Announcement hiển thị ở đâu? | [WaveAnnouncementUI.cs](Assets/Scripts/UI/WaveAnnouncementUI.cs) | `ShowWave()` | Nhận WaveStarted từ controller và trình bày số Wave, tổng Wave, tên Wave. | `WAVE ANNOUNCEMENT` |

## Boss và kết thúc trận Boss

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
| Boss xuất hiện ở đâu? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `Start()`, `spawnedBoss = Instantiate(...)` | Sau toàn bộ Wave và thời gian cảnh báo, controller sinh bossPrefab của Scene. | `spawnedBoss = Instantiate` |
| Boss Warning trước khi sinh nằm ở đâu? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs), [WaveAnnouncementUI.cs](Assets/Scripts/UI/WaveAnnouncementUI.cs) | `NotifyBossWarning()`, `ShowBossWarning()` | Controller phát sự kiện kèm thời gian nghỉ; UI hiện WARNING/BOSS INCOMING trong khoảng đó. | `BOSS WARNING UI` trong WaveAnnouncementUI |
| Boss nhận HP ở đâu? | [Boss.cs](Assets/Scripts/Boss/Boss.cs) | `Start()`, `maxHealth`, `currentHealth` | Start sao chép maxHealth từ Prefab vào HP hiện tại. | `currentHealth = maxHealth` |
| Đạn Player làm Boss mất máu ở đâu? | [Bullet.cs](Assets/Scripts/Combat/Bullet.cs), [Boss.cs](Assets/Scripts/Boss/Boss.cs) | `Bullet.OnTriggerEnter2D()`, `Boss.TakeDamage()` | Đạn gọi TakeDamage; Boss.cs thực sự giảm currentHealth. | `BOSS TRU HP` trong Boss.cs |
| Boss bay vào và đổi hướng ở đâu? | [BossMovement.cs](Assets/Scripts/Boss/BossMovement.cs) | `Update()`, `IsInPosition`, `horizontalDirection` | MoveTowards tới battleHeight; sau đó di chuyển ngang và đảo chiều ở giới hạn Camera. | `BOSS DOI HUONG` |
| Vì sao Boss chưa vào đủ thì chưa bắn? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `Update()`, `bossMovement.IsInPosition` | Guard trả về khi chưa vào vị trí; chu kỳ còn có initialAttackDelay trước phát đầu. | `!bossMovement.IsInPosition` |
| Boss chọn kỹ năng như thế nào? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `CurrentPattern`, `ActiveCycle`, `UpdateAttackCycle()` | Lấy phần tử theo cycleIndex từ chu kỳ cấu hình và xử lý theo trạng thái đòn đang dở. | `void UpdateAttackCycle` |
| SingleShot và SpreadShot tạo đạn ở đâu? | [BossBullet.cs](Assets/Scripts/Boss/BossBullet.cs) | `FirePattern()` | Single sinh một viên giữa; Spread sinh thêm hai viên lệch góc, thành ba hướng. | `BOSS SPREAD SHOT` |
| AimedShot khóa Player lúc nào? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `UpdateAttackCycle()`, `AimedTargetPosition` | Lưu tọa độ Player khi bắt đầu cảnh báo; sau đó bắn tới tọa độ cũ, không homing. | `AimedTargetPosition = targetPlayer.transform.position` |
| BurstShot khác SpreadShot thế nào? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs), [BossBullet.cs](Assets/Scripts/Boss/BossBullet.cs) | `FireBurstShot()`, `FirePattern()` | Burst bắn từng viên theo thời gian; Spread sinh ba viên theo ba hướng trong một lần gọi. | `BOSS BURST SHOT` trong BossShooting |
| Final Boss quét đạn ở đâu? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `FireSweepShot()` | Lerp góc quét, bắn từng viên theo sweepShotInterval và đảo chiều ở lượt sau. | `void FireSweepShot` |
| Final Boss chuyển Phase ở đâu? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `Update()`, `phase2HealthFraction`, `IsPhase2` | Khi HP tới ngưỡng max HP nhân tỉ lệ và chưa ở Phase 2 thì chuyển đúng một lần. | `BOSS PHASE 2` |
| Cảnh báo đòn ngắm/quét có phải Boss Warning UI không? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `ShowWarning()` | Đây là viền Sprite trên thân Boss, khác banner báo Boss sắp xuất hiện của WaveAnnouncementUI. | `void ShowWarning` |
| Thanh HP Boss biết đúng Boss bằng cách nào? | [BossHealthUI.cs](Assets/Scripts/UI/BossHealthUI.cs) | `OnEnable()`, `ShowBossHealth()`, `Update()` | Nghe BossSpawned, đặt max/value Slider rồi đọc CurrentHealth mỗi frame. | `BOSS HEALTH BAR` |
| Boss chết cộng điểm và kích hoạt Victory thế nào? | [Boss.cs](Assets/Scripts/Boss/Boss.cs), [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `TakeDamage()`, `Start()`, `ShowVictory()` | Boss cộng scoreValue rồi Destroy; controller chờ đúng Boss này bị hủy mới xét gọi Victory. | `BOSS KET THUC VA VICTORY` trong Level1Controller |
| Boss chết hoặc Game Over rồi có bắn tiếp không? | [BossShooting.cs](Assets/Scripts/Boss/BossShooting.cs) | `Update()`, `CancelPendingAttack()`, `OnDisable()` | Nhánh chu kỳ hủy trạng thái ngắm/Burst/Sweep; đạn đã bắn trước đó có vòng đời riêng trong BossBullet. | `void CancelPendingAttack` |

## Power-up, Score, UI và điều hướng

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
| Xác suất Enemy rơi đồ được xét ở đâu? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `OnEnemyDefeated()`, `weaponDropChance`, `healthDropChance` | Bốc một số ngẫu nhiên, chọn súng/máu theo các khoảng xác suất; phần còn lại không rơi. | `XAC SUAT ROI VAT PHAM` |
| Khi nào Enemy chết nhưng không rơi vật phẩm? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `OnEnemyDefeated()`, `CanSpawn()` | Có thể không trúng xác suất, đủ hạn mức, sai Scene, hết Wave, trận kết thúc, vị trí không an toàn hoặc bị lịch Wave chặn. | `POWER UP CHAN ROI` |
| Lịch vật phẩm theo Wave ở đâu? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `scheduledDrops`, `OnWaveStarted()`, `DropForWave()` | Cấu hình theo waveNumber và delay, không bốc xác suất; vẫn phải qua điều kiện sinh và điều kiện thiếu HP nếu có. | `LICH VAT PHAM THEO WAVE` |
| Chỉ dòng tạo WeaponUpgradePickup hoặc HealthPickup? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `GetPrefab()`, `Spawn()` | Chọn Prefab theo loại rồi Instantiate trong Spawn; cả rơi theo lịch và ngẫu nhiên đều dùng chung. | `SINH VAT PHAM` |
| Vật phẩm rơi xuống và tự biến mất ở đâu? | [PowerUpPickup.cs](Assets/Scripts/PowerUps/PowerUpPickup.cs) | `Update()`, `Start()`, `fallSpeed`, `maximumLifetime` | Di chuyển xuống theo deltaTime, hủy khi ra màn hình hoặc hết tuổi thọ. | `void Update` |
| Player nhặt vật phẩm ở đâu? | [PowerUpPickup.cs](Assets/Scripts/PowerUps/PowerUpPickup.cs) | `OnTriggerEnter2D()` | Chỉ nhận Player còn sống thuộc manager, sau đó phát Collected và hủy vật phẩm. | `void OnTriggerEnter2D` |
| Vì sao một vật phẩm không được nhặt nhiều lần? | [PowerUpPickup.cs](Assets/Scripts/PowerUps/PowerUpPickup.cs), [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `IsCollected`, `OnCollected()`, `activePickups` | Pickup đặt cờ/tắt Collider; manager Remove khỏi HashSet trước khi báo hiệu ứng. | `VAT PHAM CHAN NHAT TRUNG` trong PowerUpPickup |
| Nhặt vật phẩm tăng súng ở đâu? | [PlayerPowerUpEffects.cs](Assets/Scripts/Player/PlayerPowerUpEffects.cs) | `ApplyPickup()` | Nhận PickupCollected, xác nhận đúng Player rồi gọi PlayerShooting.UpgradeWeapon. | `NHAT NANG SUNG` |
| Nhặt vật phẩm hồi đúng 1 HP ở đâu? | [PlayerPowerUpEffects.cs](Assets/Scripts/Player/PlayerPowerUpEffects.cs) | `ApplyPickup()`, `playerHealth.Heal(1)` | HealthPickup gọi Heal(1); Heal tự chặn vượt max HP. | `NHAT HOI 1 HP` |
| Cấp súng được vẽ lên HUD ở đâu? | [WeaponLevelUI.cs](Assets/Scripts/UI/WeaponLevelUI.cs) | `ShowLevel()`, `WeaponLevelChanged` | Sự kiện mang cấp mới đến ShowLevel để đổi TMP_Text. | `CAP NHAT WEAPON UI` |
| Đâu là nơi cộng điểm thật? | [ScoreManager.cs](Assets/Scripts/Managers/ScoreManager.cs) | `AddScore()`, `CurrentScore` | Cộng lượng điểm dương rồi phát ScoreChanged; Enemy/Asteroid/Boss chỉ gọi hàm này. | `CONG SCORE` |
| Điểm hiển thị trên màn hình ở đâu? | [ScoreUI.cs](Assets/Scripts/UI/ScoreUI.cs) | `OnEnable()`, `UpdateScoreText()` | Nghe ScoreChanged để cập nhật chữ SCORE, đồng thời hiển thị điểm hiện tại khi UI bật. | `CAP NHAT SCORE UI` |
| Pause/Resume hoạt động thế nào? | [PauseManager.cs](Assets/Scripts/Managers/PauseManager.cs) | `TogglePause()`, `SetPaused()` | Đảo IsPaused và đặt Time.timeScale về 0 hoặc 1; Escape và nút Pause cùng dùng cơ chế này. | `PAUSE RESUME` |
| Vì sao thắng/thua không thể Escape để chơi tiếp? | [PauseManager.cs](Assets/Scripts/Managers/PauseManager.cs) | `LockPause()`, `isPauseLocked` | GameResultManager khóa Pause; TogglePause không đổi trạng thái khi đã khóa. | `public void LockPause` |
| Game Over và Victory có thể hiện đồng thời không? | [GameResultManager.cs](Assets/Scripts/Managers/GameResultManager.cs) | `FinishGame()`, `IsGameFinished` | Chỉ nhận kết quả đầu tiên; bật Victory bằng victory và Game Over bằng !victory rồi dừng thời gian. | `HIEN VICTORY PANEL` |
| Retry reset HP, súng, điểm và Wave ở đâu? | [GameRetry.cs](Assets/Scripts/Managers/GameRetry.cs) | `PlayAgain()` | Trả timeScale về 1 và tải lại Scene hiện tại ở chế độ Single; object mới tự khởi tạo lại. | `RETRY LOAD SCENE` |
| NEXT LEVEL tải Scene nào? | [LevelNavigation.cs](Assets/Scripts/Managers/LevelNavigation.cs) | `LoadNextLevel()`, `nextSceneName` | Dùng tên Scene cấu hình trong Inspector, kiểm tra có trong Build Settings rồi tải bằng Single. | `CHUYEN LEVEL` |
| Quay về Main Menu ở đâu? | [SceneMenuNavigation.cs](Assets/Scripts/Managers/SceneMenuNavigation.cs) | `ReturnToMainMenu()`, `LoadScene()` | ReturnToMainMenu truyền MainMenuScene vào hàm tải chung; hàm này reset timeScale trước khi chuyển. | `MENU LOAD SCENE` |
| Nút chọn Level hoạt động ra sao? | [SceneMenuNavigation.cs](Assets/Scripts/Managers/SceneMenuNavigation.cs) | `ShowLevelSelect()`, `Play()`, `LoadLevel2()`, `LoadLevel3()` | Hiện khu vực chọn màn; mỗi nút gọi hàm tải Scene tương ứng. | `MO CHON LEVEL` |
| QUIT trong Editor có đóng Editor không? | [SceneMenuNavigation.cs](Assets/Scripts/Managers/SceneMenuNavigation.cs) | `QuitGame()`, `#if !UNITY_EDITOR` | Không; Application.Quit chỉ được đưa vào bản biên dịch ngoài Editor. | `public void QuitGame` |

## Unity C# cơ bản ngay trong project

| Câu hỏi giảng viên | File cần mở | Tên hàm hoặc biến cần chỉ | Câu trả lời ngắn gọn, dễ nhớ | Từ khóa tìm bằng Ctrl+F |
| Awake và Start dùng khác nhau thế nào ở đây? | [PlayerShooting.cs](Assets/Scripts/Player/PlayerShooting.cs) | `Awake()`, `Start()` | Awake lấy component cùng Player; Start kiểm tra Prefab/FirePoint. Unity gọi Awake trước Start của instance đó. | `void Awake` |
| Update dùng làm gì? | [PlayerMovement.cs](Assets/Scripts/Player/PlayerMovement.cs) | `Update()` | Callback chạy mỗi frame khi component hoạt động để đọc input và cập nhật di chuyển. | `void Update` |
| Coroutine có phải luồng chạy riêng không? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs) | `SpawnWave()`, `StartCoroutine` | Không; coroutine chia công việc qua các lần tiếp tục trên main thread của Unity. | `IEnumerator SpawnWave` |
| WaitForSeconds và WaitUntil khác gì? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs) | `SpawnWave()` | WaitForSeconds chờ thời gian game, WaitUntil chờ điều kiện đúng; ở đây lần lượt dùng giãn spawn và chờ hết Enemy. | `yield return new WaitUntil` |
| SerializeField có tác dụng gì? | [Level1Controller.cs](Assets/Scripts/Managers/Level1Controller.cs) | `customWaves`, `[SerializeField]` | Cho Unity serialize field private đủ điều kiện để cấu hình trong Inspector; không biến nó thành public API. | `[SerializeField] private WaveDefinition[]` |
| GetComponent và GetComponentInParent khác gì trong game? | [PowerUpPickup.cs](Assets/Scripts/PowerUps/PowerUpPickup.cs), [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `OnTriggerEnter2D()` | Pickup tìm PlayerHealth trên object va chạm hoặc cha; Bullet tìm Enemy trên chính object va chạm. | `GetComponentInParent<PlayerHealth>` trong PowerUpPickup |
| Vì sao phải gỡ sự kiện OnDisable? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `OnEnable()`, `OnDisable()` | Cân bằng += và -=, nhất là Enemy.Defeated static, để manager cũ không nhận callback sau đổi Scene. | `Enemy.Defeated -= OnEnemyDefeated` |
| HashSet dùng để làm gì? | [PowerUpDropManager.cs](Assets/Scripts/PowerUps/PowerUpDropManager.cs) | `observedWaves`, `activePickups`, `OnCollected()` | Theo dõi phần tử không trùng; Remove trả false thì vật phẩm không được tính lại. | `if (!activePickups.Remove(pickup))` |
| Vì sao reference tới object đã Destroy có thể so với null? | [EnemySpawner.cs](Assets/Scripts/Enemy/EnemySpawner.cs) | `AllEnemiesDestroyed()` | UnityEngine.Object có phép so sánh đặc biệt để object Unity đã bị hủy được xem như null. | `if (enemy != null)` |
| Bounds, Collider và Trigger có giống nhau không? | [EnemyFormationSpawner.cs](Assets/Scripts/Enemy/EnemyFormationSpawner.cs), [Bullet.cs](Assets/Scripts/Combat/Bullet.cs) | `SpawnIsClear()`, `OnTriggerEnter2D()` | Bounds là hộp bao để đo/kiểm tra; Collider tham gia physics; Trigger phát callback mà không tạo phản ứng đẩy như va chạm thường. | `Physics2D.OverlapBoxAll` trong EnemyFormationSpawner |
| Hiệu ứng đỏ có trực tiếp trừ HP không? | [DamageFlash.cs](Assets/Scripts/Effects/DamageFlash.cs) | `Flash()`, `RestoreColor()` | Không; DamageFlash chỉ đổi rồi khôi phục màu, còn TakeDamage ở các đối tượng mới trừ HP. | `HIEU UNG TRUNG DAN` |
| Cuộn nền có sinh liên tục object mới không? | [BackgroundScroller.cs](Assets/Scripts/Environment/BackgroundScroller.cs) | `Start()`, `Update()` | Không; hai nền được xếp nối nhau và chuyển nền ra khỏi màn hình lên phía trên để tái sử dụng. | `TAI SU DUNG NEN` |

## Danh mục 34 script đã rà soát

- Player: `PlayerMovement.cs`, `PlayerShooting.cs`, `PlayerHealth.cs`, `PlayerPowerUpEffects.cs`.
- Combat: `Bullet.cs`.
- Enemy: `Enemy.cs`, `EnemyMovement.cs`, `EnemyShooting.cs`, `EnemySpawner.cs`, `EnemyFormationSpawner.cs`, `EnemyFormationMember.cs`.
- Boss: `Boss.cs`, `BossMovement.cs`, `BossShooting.cs`, `BossBullet.cs`.
- Asteroid: `Asteroid.cs`, `AsteroidMovement.cs`, `AsteroidSpawner.cs`.
- PowerUps: `PowerUpDropManager.cs`, `PowerUpPickup.cs`.
- Managers: `Level1Controller.cs`, `ScoreManager.cs`, `PauseManager.cs`, `GameResultManager.cs`, `GameRetry.cs`, `LevelNavigation.cs`, `SceneMenuNavigation.cs`.
- UI: `PlayerHealthUI.cs`, `BossHealthUI.cs`, `ScoreUI.cs`, `WeaponLevelUI.cs`, `WaveAnnouncementUI.cs`.
- Effects/Environment: `DamageFlash.cs`, `BackgroundScroller.cs`.

Không có script mới được tạo để bổ sung chức năng. Các thay đổi C# của nhiệm vụ này chỉ là chú thích; kiểm tra Unity Compile/Console chỉ được ghi nhận khi có công cụ Editor thực sự khả dụng.
