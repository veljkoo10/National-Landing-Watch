// statistics-component.ts
import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';

// Highcharts Angular (v5 standalone component)
import * as HighchartsLib from 'highcharts';
import { HighchartsChartComponent } from 'highcharts-angular';

// Angular Material
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

// Service + DTOs
import {
  StatisticsService,
  WasteByRegionDto,
  LandfillTypeDto,
  Ch4OverTimeDto,
  TopLandfillDto,
  MostImpactedRegionFullDto,
} from '../../services';

// RxJS
import { Subject, of } from 'rxjs';
import { takeUntil, catchError, map } from 'rxjs/operators';

// ===== View models that match your HTML =====
type TopLandfillView = {
  name: string;
  region: string;
  totalWaste: number; // mapped from areaM2 for now (UI expects totalWaste)
  areaM2?: number;
  yearCreated?: number;
};

type MostImpactedView = {
  region: string;
  ch4PerKm2: number;
  co2eqPerKm2: number;
  totalCh4: number;
  totalCo2eq: number;
  population: number;
  areaKm2: number;
};

type TableRow = {
  landfillName: string;
  regionName: string;
  year: number;
  volumeM3: number;
  wasteTons: number;
  ch4Tons: number;
  co2eqTons: number;
};

@Component({
  selector: 'app-statistics',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    HighchartsChartComponent, // required even if provided globally
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    TranslatePipe,
  ],
  templateUrl: './statistics-component.html',
  styleUrls: ['./statistics-component.css'],
})
export class StatisticsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ===== Highcharts options + update flags =====
  chartRegionWaste: HighchartsLib.Options = { accessibility: { enabled: false }, series: [] };
  chartSpeciesPie: HighchartsLib.Options = { accessibility: { enabled: false }, series: [] };
  chartEmissionsTrend: HighchartsLib.Options = { accessibility: { enabled: false }, series: [] };
  chartPrediction: HighchartsLib.Options = { accessibility: { enabled: false }, series: [] };

  updateRegionWaste = false;
  updateTypes = false;
  updateEmissions = false;
  updatePrediction = false;

  // ===== Cards data =====
  topLargestLandfills: TopLandfillView[] = [];
  mostPollutedRegion?: MostImpactedView;

  // ===== Table =====
  displayedColumns: string[] = [
    'landfill',
    'region',
    'year',
    'volumeM3',
    'wasteTons',
    'ch4Tons',
    'co2eqTons',
  ];
  dataSource = new MatTableDataSource<TableRow>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private statisticsService: StatisticsService, private cdr: ChangeDetectorRef) {}

  // ===== utils =====
  private n = (v: unknown, d = 0) => {
    const num = Number(v);
    return Number.isFinite(num) ? num : d;
  };

  /** Replace chart options and flip the update flag (also markForCheck for OnPush) */
  private setChart(
    target: 'RegionWaste' | 'Types' | 'Emissions' | 'Prediction',
    opts: HighchartsLib.Options
  ) {
    switch (target) {
      case 'RegionWaste':
        this.chartRegionWaste = opts;
        this.updateRegionWaste = !this.updateRegionWaste;
        break;
      case 'Types':
        this.chartSpeciesPie = opts;
        this.updateTypes = !this.updateTypes;
        break;
      case 'Emissions':
        this.chartEmissionsTrend = opts;
        this.updateEmissions = !this.updateEmissions;
        break;
      case 'Prediction':
        this.chartPrediction = opts;
        this.updatePrediction = !this.updatePrediction;
        break;
    }
    this.cdr.markForCheck();
  }

  /** Simple linear projection on (year, value) to produce +5 years */
  private linearProjection(xs: number[], ys: number[], horizon = 5) {
    const n = Math.min(xs.length, ys.length);
    if (n < 2) return { labels: xs.map(String), hist: ys, proj: [] as number[] };

    const x = xs.slice(0, n);
    const y = ys.slice(0, n);
    const sum = (a: number[]) => a.reduce((s, v) => s + v, 0);
    const sumX = sum(x);
    const sumY = sum(y);
    const sumXY = x.reduce((s, v, i) => s + v * y[i], 0);
    const sumXX = x.reduce((s, v) => s + v * v, 0);
    const denom = n * sumXX - sumX * sumX || 1;
    const b = (n * sumXY - sumX * sumY) / denom;
    const a = (sumY - b * sumX) / n;

    const last = x[n - 1];
    const fut = Array.from({ length: horizon }, (_, i) => last + i + 1);
    const proj = fut.map((yr) => +(a + b * yr).toFixed(2));
    const labels = [...x.map(String), ...fut.map(String)];
    return { labels, hist: y, proj };
  }

  // ===== init =====
  ngOnInit(): void {
    // --- Chart 1: total waste by region ---
    this.statisticsService
      .getTotalWasteByRegion()
      .pipe(
        takeUntil(this.destroy$),
        map((rows) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as WasteByRegionDto[]))
      )
      .subscribe((rows) => {
        const categories = rows.map((r) => r?.name ?? 'N/A');
        const values = rows.map((r) => this.n(r?.totalWaste));
        this.setChart('RegionWaste', {
          accessibility: { enabled: false },
          chart: { type: 'column' },
          title: { text: undefined },
          xAxis: { categories },
          yAxis: { title: { text: 't' } },
          tooltip: { pointFormat: '{series.name}: <b>{point.y:.0f} t</b>' },
          credits: { enabled: false },
          series: [{ type: 'column', name: 'Waste (t)', data: values }],
        });
      });

    // --- Chart 2: landfill types (pie) ---
    this.statisticsService
      .getLandfillTypes()
      .pipe(
        takeUntil(this.destroy$),
        map((rows) => (Array.isArray(rows) ? rows : [])),
        catchError(() => of([] as LandfillTypeDto[]))
      )
      .subscribe((rows) => {
        this.setChart('Types', {
          accessibility: { enabled: false },
          chart: { type: 'pie' },
          title: { text: undefined },
          tooltip: { pointFormat: '<b>{point.y:.0f}</b>' },
          credits: { enabled: false },
          series: [
            {
              type: 'pie',
              name: 'Count',
              data: rows.map((r) => ({ name: r?.name ?? 'N/A', y: this.n(r?.count) })),
            },
          ],
        });
      });

    // --- CH4 Over Time & Prediction (single source, two charts) ---
    this.statisticsService
      .getCh4EmissionsOverTime()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of({ years: [], ch4ByYear: [] } as Ch4OverTimeDto)),
        map((res) => {
          const rawYears = Array.isArray(res?.years) ? res.years : [];
          const rawVals = Array.isArray(res?.ch4ByYear) ? res.ch4ByYear : [];

          // Fallback: replace invalid or 0 years with last valid year, else 2024
          let fallback = 2024;
          const validYears = rawYears.map(Number).filter((y) => Number.isFinite(y) && y > 0);
          if (validYears.length) fallback = Math.max(...validYears);

          const xs = rawYears.map((v) => {
            const n = Number(v);
            return Number.isFinite(n) && n > 0 ? n : fallback;
          });
          const ys = rawVals.map((v) => this.n(v));
          const n = Math.min(xs.length, ys.length);

          return { xs: xs.slice(0, n), ys: ys.slice(0, n) };
        })
      )
      .subscribe(({ xs, ys }) => {
        // 3a) Emissions Over Time
        const categories = xs.length ? xs.map(String) : ['2022', '2023', '2024'];
        const seriesData = xs.length ? ys : [0, 0, 0];

        this.setChart('Emissions', {
          accessibility: { enabled: false },
          chart: { type: 'line' },
          title: { text: undefined },
          xAxis: { categories },
          yAxis: { title: { text: 'CH₄ (t)' }, min: 0 },
          tooltip: { shared: true, valueDecimals: 0 },
          credits: { enabled: false },
          series: [{ type: 'line', name: 'CH₄', data: seriesData }],
        });

        // 3b) Prediction from same data
        if (xs.length < 2) {
          this.setChart('Prediction', {
            accessibility: { enabled: false },
            chart: { type: 'line' },
            title: { text: undefined },
            xAxis: { categories: [] },
            yAxis: { title: { text: 'CH₄ (t)' } },
            series: [{ type: 'line', name: 'Projected CH₄', data: [] }],
            subtitle: { text: 'Not enough data to project (need ≥ 2 years).' },
            credits: { enabled: false },
          });
        } else {
          const { labels, hist, proj } = this.linearProjection(xs, ys, 5);
          const lastHist = hist.length ? hist[hist.length - 1] : null;

          this.setChart('Prediction', {
            accessibility: { enabled: false },
            chart: { type: 'line' },
            title: { text: undefined },
            xAxis: { categories: labels },
            yAxis: { title: { text: 'CH₄ (t)' }, min: 0 },
            tooltip: { shared: true },
            credits: { enabled: false },
            series: [
              { type: 'line', name: 'Historical CH₄', data: hist },
              {
                type: 'line',
                name: 'Projected CH₄',
                data: [...Array(Math.max(hist.length - 1, 0)).fill(null), lastHist, ...proj],
                ...({ dashStyle: 'ShortDash' } as any),
              },
            ],
          });
        }
      });

    // --- Section 2: top 3 largest (map areaM2 -> totalWaste for UI) ---
    this.statisticsService
      .getTop3LargestLandfills()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of([] as TopLandfillDto[])),
        map((rows) =>
          (Array.isArray(rows) ? rows : []).map(
            (r) =>
              ({
                name: r?.name ?? 'N/A',
                region: r?.region ?? 'Unknown',
                totalWaste: this.n(r?.areaM2), // HTML expects 't', but we show 'm²' in template
                areaM2: r?.areaM2,
                yearCreated: r?.yearCreated,
              } as TopLandfillView)
          )
        )
      )
      .subscribe((rows) => {
        this.topLargestLandfills = rows;
        this.cdr.markForCheck();
      });

    // --- Section 2: most impacted (full DTO) ---
    this.statisticsService
      .getMostImpactedRegion()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() =>
          of({
            region: 'Unknown',
            totalCh4: 0,
            totalCo2eq: 0,
            ch4PerKm2: 0,
            co2eqPerKm2: 0,
            population: 0,
            areaKm2: 0,
          } as MostImpactedRegionFullDto)
        )
      )
      .subscribe((mp) => {
        this.mostPollutedRegion = mp;
        this.cdr.markForCheck();
      });

    // --- Section 4: table (stats endpoint) ---
    this.statisticsService
      .getLandfillStats()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => of([] as TableRow[]))
      )
      .subscribe((rows) => {
        this.dataSource = new MatTableDataSource<TableRow>(rows ?? []);

        // Sorting: case-insensitive for text, numeric for numbers
        this.dataSource.sortingDataAccessor = (row: TableRow, columnId: string) => {
          switch (columnId) {
            case 'landfill':
              return (row.landfillName ?? '').toLowerCase();
            case 'region':
              return (row.regionName ?? '').toLowerCase();
            case 'year':
              return Number(row.year ?? 0);
            case 'volumeM3':
              return Number(row.volumeM3 ?? 0);
            case 'wasteTons':
              return Number(row.wasteTons ?? 0);
            case 'ch4Tons':
              return Number(row.ch4Tons ?? 0);
            case 'co2eqTons':
              return Number(row.co2eqTons ?? 0);
            default:
              return '';
          }
        };

        // Filter: search across all columns
        this.dataSource.filterPredicate = (data: TableRow, raw: string) => {
          const f = (raw ?? '').trim().toLowerCase();
          if (!f) return true;
          return (
            `${data.landfillName ?? ''} ${data.regionName ?? ''} ${data.year ?? ''} ` +
            `${data.volumeM3 ?? ''} ${data.wasteTons ?? ''} ${data.ch4Tons ?? ''} ${
              data.co2eqTons ?? ''
            }`
          )
            .toLowerCase()
            .includes(f);
        };

        // Hook up paginator/sort
        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;

        this.cdr.markForCheck();
      });
  }

  applyFilter(value: string) {
    this.dataSource.filter = (value ?? '').trim().toLowerCase();
  }
}
