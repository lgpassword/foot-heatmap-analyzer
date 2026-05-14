import { FootHeatmapApplication } from "./app/FootHeatmapApplication.js";

// 页面入口只负责创建应用对象并启动，具体职责由各模块类承担。
const application = new FootHeatmapApplication(document);

// 启动页面交互、图表渲染、上传、导出和设置功能。
application.start();
