// 负责把租户档案管理和硬件接入 API 暴露到页面上。
export class PlatformController {
    // 保存当前文档对象。
    document;

    // 保存扫描数据仓储。
    dataStore;

    // 保存提示服务。
    toastService;

    // 创建平台功能控制器。
    constructor(document, dataStore, toastService) {
        this.document = document;
        this.dataStore = dataStore;
        this.toastService = toastService;
    }

    // 绑定页面平台动作按钮。
    bind() {
        for (const button of this.document.querySelectorAll("[data-platform-action]")) {
            button.addEventListener("click", () => this.run(button.dataset.platformAction ?? ""));
        }

        this.loadProfiles();
    }

    // 分发平台动作。
    run(actionName) {
        if (actionName === "create-profile") {
            this.createProfile();
            return;
        }

        if (actionName === "submit-hardware") {
            this.submitHardwareScan();
        }
    }

    // 创建当前租户下的患者或运动员档案。
    async createProfile() {
        const tenantId = this.inputValue("tenantIdInput", "demo");
        const kind = this.inputValue("profileKindInput", "Patient");
        const displayName = this.inputValue("profileNameInput", "演示档案");
        const response = await fetch("/api/profiles", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Tenant-Id": tenantId
            },
            body: JSON.stringify({ kind, displayName, dateOfBirth: null, notes: "Created from dashboard UI" })
        });

        if (!response.ok) {
            this.toastService.show("档案创建失败。");
            return;
        }

        this.toastService.show("档案已创建。");
        await this.loadProfiles();
    }

    // 加载当前租户档案列表。
    async loadProfiles() {
        const tenantId = this.inputValue("tenantIdInput", "demo");
        const response = await fetch("/api/profiles", { headers: { "X-Tenant-Id": tenantId } });
        if (!response.ok) return;

        const profiles = await response.json();
        const list = this.document.getElementById("profileList");
        if (!list) return;

        list.innerHTML = profiles.length === 0
            ? `<span class="empty-state">当前租户暂无档案</span>`
            : profiles.map(profile => `
                <button type="button" class="profile-pill" data-profile-id="${profile.id}">
                    <strong>${profile.displayName}</strong>
                    <span>${profile.kind} · ${profile.id.slice(0, 8)}</span>
                </button>`).join("");

        for (const item of list.querySelectorAll("[data-profile-id]")) {
            item.addEventListener("click", () => {
                const input = this.document.getElementById("hardwareProfileIdInput");
                if (input) input.value = item.dataset.profileId ?? "";
            });
        }
    }

    // 调用开放硬件接入 API，模拟设备上传当前扫描。
    async submitHardwareScan() {
        const payload = this.dataStore.getPayload();
        if (!payload) return;

        const request = {
            deviceId: this.inputValue("deviceIdInput", "demo-device"),
            tenantId: this.inputValue("tenantIdInput", "demo"),
            profileId: this.inputValue("hardwareProfileIdInput", ""),
            payloadEncoding: "hex",
            payload: this.encodeCurrentPayloadAsHex(payload),
            capturedAt: new Date().toISOString()
        };
        const response = await fetch("/api/hardware/scans", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(request)
        });
        const result = await response.json();
        const output = this.document.getElementById("hardwareApiResult");
        if (output) {
            output.textContent = JSON.stringify({
                scanId: result.scanId,
                deviceId: result.deviceId,
                tenantId: result.tenantId,
                profileId: result.profileId,
                archType: result.report?.archType,
                balance: result.dashboard?.loadBalance
            }, null, 2);
        }

        this.toastService.show(response.ok ? "硬件扫描已接入平台。" : "硬件接入失败。");
    }

    // 读取输入控件值。
    inputValue(id, fallback) {
        return this.document.getElementById(id)?.value?.trim() || fallback;
    }

    // 将当前矩阵编码为演示协议十六进制载荷。
    encodeCurrentPayloadAsHex(payload) {
        const height = payload.left.length;
        const width = payload.left[0].length;
        const bytes = [width, height];
        for (const matrix of [payload.left, payload.right]) {
            for (const row of matrix) {
                for (const value of row) {
                    bytes.push(Math.max(0, Math.min(255, Math.round(value * 255))));
                }
            }
        }

        return bytes.map(value => value.toString(16).padStart(2, "0")).join("");
    }
}
