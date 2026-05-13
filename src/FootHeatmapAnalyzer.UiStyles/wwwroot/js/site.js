const workspace = document.querySelector(".workspace");
const sampleButton = document.getElementById("loadSample");
const scanText = document.getElementById("ScanText");

if (sampleButton && scanText && workspace?.dataset.sample) {
    sampleButton.addEventListener("click", () => {
        scanText.value = workspace.dataset.sample;
    });
}

if (workspace?.dataset.heatmap) {
    const payload = JSON.parse(workspace.dataset.heatmap);
    drawHeatmap("leftHeatmap", payload.left);
    drawHeatmap("rightHeatmap", payload.right);
}

function drawHeatmap(canvasId, matrix) {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !matrix?.length) return;

    const ctx = canvas.getContext("2d");
    const rows = matrix.length;
    const cols = matrix[0].length;
    const cellWidth = canvas.width / cols;
    const cellHeight = canvas.height / rows;

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    for (let y = 0; y < rows; y++) {
        for (let x = 0; x < cols; x++) {
            ctx.fillStyle = colorFor(matrix[y][x]);
            ctx.fillRect(x * cellWidth, y * cellHeight, Math.ceil(cellWidth), Math.ceil(cellHeight));
        }
    }
}

function colorFor(value) {
    const v = Math.max(0, Math.min(1, value));
    const hue = 220 - (220 * v);
    const lightness = 18 + (42 * v);
    return `hsl(${hue}, 92%, ${lightness}%)`;
}
