// 负责读取 Razor 输出的惰性 JSON 数据块，不处理任何 UI 行为。
export class HeatmapDataStore {
    // 保存当前页面文档对象。
    document;

    // 保存解析后的左右足热力图矩阵。
    payload;

    // 创建数据仓储并立即读取页面数据。
    constructor(document) {
        this.document = document;
        this.payload = this.readHeatmapPayload();
    }

    // 返回当前扫描数据，供渲染器和导出器复用。
    getPayload() {
        return this.payload;
    }

    // 从 application/json 脚本块中读取左右足矩阵。
    readHeatmapPayload() {
        const heatmapData = this.document.getElementById("heatmap-data");
        if (!heatmapData?.textContent) return null;

        return JSON.parse(heatmapData.textContent);
    }
}
