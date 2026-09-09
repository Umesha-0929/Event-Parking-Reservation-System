export interface StagePatternSeat {
  seatId: string;
  rowLabel: string;
  seatNumber: string;
}

export interface StagePatternPosition<T extends StagePatternSeat = StagePatternSeat> {
  seat: T;
  x: number;
  y: number;
}

export interface StageGeometry {
  x: number;
  y: number;
  w: number;
  h: number;
}

export function stageTemplateGeometry(stageType: number): StageGeometry {
  switch (stageType) {
    case 1: return { x: 350, y: 230, w: 200, h: 150 }; // Arena: centre stage, four audience banks.
    case 2: return { x: 255, y: 505, w: 390, h: 75 };  // Proscenium: stage at one end, fan seating faces it.
    case 3: return { x: 250, y: 55, w: 400, h: 95 };   // End-On: audience faces one end.
    case 4: return { x: 350, y: 55, w: 200, h: 260 };  // Thrust: stage projects into audience on three sides.
    case 5: return { x: 145, y: 260, w: 610, h: 100 }; // Traverse: long stage with two opposing audience banks.
    case 6: return { x: 365, y: 245, w: 170, h: 130 }; // In-the-Round: central stage with surrounding rings.
    default: return { x: 255, y: 505, w: 390, h: 75 };
  }
}

export function stagePatternPositions<T extends StagePatternSeat>(
  seats: T[],
  stageType: number,
  width: number,
  height: number,
): StagePatternPosition<T>[] {
  const clamp = (value: number, min: number, max: number) => Math.max(min, Math.min(max, value));
  const safe = (seat: T, x: number, y: number): StagePatternPosition<T> => ({
    seat,
    x: Number(clamp(x, 28, width - 28).toFixed(2)),
    y: Number(clamp(y, 28, height - 28).toFixed(2)),
  });
  const seatNumberValue = (value: string): number => {
    const number = Number(String(value).replace(/\D+/g, ''));
    return Number.isFinite(number) ? number : 0;
  };
  const ordered = [...seats].sort((a, b) =>
    a.rowLabel.localeCompare(b.rowLabel, undefined, { numeric: true }) ||
    seatNumberValue(a.seatNumber) - seatNumberValue(b.seatNumber));
  const rowMap = new Map<string, T[]>();
  for (const seat of ordered) {
    const row = rowMap.get(seat.rowLabel) ?? [];
    row.push(seat);
    rowMap.set(seat.rowLabel, row);
  }
  const rows = [...rowMap.entries()]
    .sort(([a], [b]) => a.localeCompare(b, undefined, { numeric: true }))
    .map(([, row]) => row.sort((a, b) => seatNumberValue(a.seatNumber) - seatNumberValue(b.seatNumber)));

  // Proscenium: curved/fan-style rows that widen away from the stage.
  if (stageType === 2) {
    const result: StagePatternPosition<T>[] = [];
    rows.forEach((row, rowIndex) => {
      const progress = rows.length > 1 ? rowIndex / (rows.length - 1) : 0;
      const y = 445 - progress * 325;
      const rowWidth = 500 + progress * 245;
      const gap = row.length > 1 ? rowWidth / (row.length - 1) : 0;
      const startX = 450 - rowWidth / 2;
      row.forEach((seat, index) => {
        const normal = row.length > 1 ? Math.abs((index / (row.length - 1)) * 2 - 1) : 0;
        const curve = normal * normal * 24;
        result.push(safe(seat, row.length === 1 ? 450 : startX + index * gap, y - curve));
      });
    });
    return result;
  }

  // End-On: conventional rows in front of a stage at one end.
  if (stageType === 3) {
    const result: StagePatternPosition<T>[] = [];
    rows.forEach((row, rowIndex) => {
      const y = 205 + rowIndex * Math.min(42, rows.length > 1 ? 330 / (rows.length - 1) : 0);
      const rowWidth = 660;
      const gap = row.length > 1 ? rowWidth / (row.length - 1) : 0;
      const startX = 450 - rowWidth / 2;
      row.forEach((seat, index) => result.push(safe(seat, row.length === 1 ? 450 : startX + index * gap, y)));
    });
    return result;
  }

  // In-the-Round: concentric elliptical rings around the centre stage.
  if (stageType === 6) {
    const result: StagePatternPosition<T>[] = [];
    rows.forEach((row, rowIndex) => {
      const radiusX = 155 + rowIndex * 34;
      const radiusY = 118 + rowIndex * 27;
      const offset = rowIndex % 2 ? Math.PI / Math.max(2, row.length) : 0;
      row.forEach((seat, index) => {
        const angle = -Math.PI / 2 + offset + (Math.PI * 2 * index) / Math.max(1, row.length);
        result.push(safe(seat, 450 + Math.cos(angle) * radiusX, 310 + Math.sin(angle) * radiusY));
      });
    });
    return result;
  }

  const bankPattern = (banks: number, mode: 'arena' | 'thrust' | 'traverse'): StagePatternPosition<T>[] => {
    const perBank = Math.ceil(ordered.length / banks);
    const groups = Array.from({ length: banks }, (_, bank) => ordered.slice(bank * perBank, (bank + 1) * perBank));
    const result: StagePatternPosition<T>[] = [];

    const placeGrid = (group: T[], x1: number, x2: number, y1: number, y2: number, preferWide: boolean) => {
      if (!group.length) return;
      const ratio = preferWide ? 1.9 : 0.62;
      const cols = Math.max(1, Math.ceil(Math.sqrt(group.length * ratio)));
      const gridRows = Math.max(1, Math.ceil(group.length / cols));
      group.forEach((seat, index) => {
        const col = index % cols;
        const row = Math.floor(index / cols);
        const x = cols === 1 ? (x1 + x2) / 2 : x1 + (x2 - x1) * col / (cols - 1);
        const y = gridRows === 1 ? (y1 + y2) / 2 : y1 + (y2 - y1) * row / (gridRows - 1);
        result.push(safe(seat, x, y));
      });
    };

    if (mode === 'arena') {
      placeGrid(groups[0], 250, 650, 55, 175, true);
      placeGrid(groups[1], 705, 835, 175, 445, false);
      placeGrid(groups[2], 250, 650, 445, 570, true);
      placeGrid(groups[3], 65, 195, 175, 445, false);
    } else if (mode === 'thrust') {
      placeGrid(groups[0], 70, 285, 125, 445, false);
      placeGrid(groups[1], 615, 830, 125, 445, false);
      placeGrid(groups[2], 275, 625, 410, 570, true);
    } else {
      placeGrid(groups[0], 105, 795, 65, 205, true);
      placeGrid(groups[1], 105, 795, 420, 560, true);
    }
    return result;
  };

  if (stageType === 1) return bankPattern(4, 'arena');
  if (stageType === 4) return bankPattern(3, 'thrust');
  if (stageType === 5) return bankPattern(2, 'traverse');

  return ordered.map((seat, index) => safe(seat, 120 + (index % 12) * 55, 220 + Math.floor(index / 12) * 42));
}
