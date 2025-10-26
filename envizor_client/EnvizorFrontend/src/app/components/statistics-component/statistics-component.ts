import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import * as Highcharts from 'highcharts';
import { HighchartsChartComponent } from 'highcharts-angular';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslatePipe } from '../../pipes/translation-pipe';
import {
  StatisticsService,
  LandfillService,
  RegionService } from '../../services';
import {
  LandfillSite,
  Ch4OverTimeDto
   } from '../../DTOs';

type TopLandfillView = {
  name: string;
  region: string;
  totalWaste: number;
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
    HighchartsChartComponent,
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
export class StatisticsComponent implements OnInit {
  chartRegionWaste: Highcharts.Options = {};
  chartSpeciesPie: Highcharts.Options = {};
  chartEmissionsTrend: Highcharts.Options = {};

  topLargestLandfills: TopLandfillView[] = [];
  mostPollutedRegion?: MostImpactedView;
  dataSource = new MatTableDataSource<TableRow>([]);
  displayedColumns = [
    'landfill',
    'region',
    'year',
    'volumeM3',
    'wasteTons',
    'ch4Tons',
    'co2eqTons',
  ];

  regionDisplayMap2: Record<string, string> = {
    beograd: 'Beograd',
    vojvodina: 'Vojvodina',
    zapadnasrbija: 'Zapadna Srbija',
    sumadijapomoravlje: 'Šumadija i Pomoravlje',
    istocnasrbija: 'Istočna Srbija',
    juznasrbija: 'Južna Srbija',
    kosovimetohija: 'Kosovo i Metohija',
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private statisticsService: StatisticsService,
    private landfillService: LandfillService,
    private regionService: RegionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const base = this.getChartBaseOptions();

    // Chart 1: Total waste by region
    this.regionService.getAllRegions().subscribe((rows) => {
      this.chartRegionWaste = {
        ...base,
        chart: { ...base.chart, type: 'column' },
        xAxis: {
          ...base.xAxis,
          categories: rows.map((r) => r.name),
          title: { text: 'Region', style: { color: (base.xAxis as any).labels.style.color } },
          labels: { rotation: -45, style: { color: (base.xAxis as any).labels.style.color } },
        },
        yAxis: {
          ...base.yAxis,
          title: { text: 'Waste (t)', style: { color: (base.yAxis as any).labels.style.color } },
        },
        series: [
          {
            type: 'column',
            name: 'Waste (t)',
            data: rows.map((r) => r.totalWaste),
          },
        ],
      };
      this.cdr.markForCheck();
    });

    // Chart 2: Landfill types
    this.landfillService
      .getAllLandfills()
      .pipe(
        map((landfills: LandfillSite[]) => {
          const typeCounts: Record<string, number> = {};
          for (const lf of landfills) {
            const typeLabel =
              lf.category === 0
                ? 'Illegal'
                : lf.category === 1
                ? 'NonSanitary'
                : lf.category === 2
                ? 'Sanitary'
                : 'Unknown';
            typeCounts[typeLabel] = (typeCounts[typeLabel] || 0) + 1;
          }
          return Object.entries(typeCounts).map(([name, y]) => ({ name, y }));
        }),
        catchError(() => of([]))
      )
      .subscribe((data) => {
        this.chartSpeciesPie = {
          ...base,
          chart: { ...base.chart, type: 'pie' },
          series: [
            {
              type: 'pie',
              name: 'Count',
              data,
            },
          ],
        };
        this.cdr.markForCheck();
      });

    // Chart 3: CH4 emissions over time
    this.statisticsService
      .getCh4EmissionsOverTime()
      .pipe(
        catchError(() => of({ years: [], ch4ByYear: [] } as Ch4OverTimeDto)),
        map((res) => ({
          years: Array.isArray(res?.years) ? res.years.map(Number) : [],
          ch4: Array.isArray(res?.ch4ByYear) ? res.ch4ByYear.map(Number) : [],
        }))
      )
      .subscribe(({ years, ch4 }) => {
        this.chartEmissionsTrend = {
          ...base,
          chart: { ...base.chart, type: 'line' },
          xAxis: { ...base.xAxis, categories: years.map(String) },
          yAxis: { ...base.yAxis, title: { text: 'CH₄ (t)' } },
          series: [{ type: 'line', name: 'CH₄', data: ch4 }],
        };
        this.cdr.markForCheck();
      });

    // Top 3 Largest Landfills
    this.landfillService
      .getAllLandfills()
      .pipe(
        map((landfills: LandfillSite[]) =>
          (landfills ?? [])
            .filter((lf) => typeof lf.estimatedAreaM2 === 'number')
            .sort((a, b) => (b.estimatedAreaM2 ?? 0) - (a.estimatedAreaM2 ?? 0))
            .slice(0, 3)
        ),
        catchError(() => of([] as LandfillSite[]))
      )
      .subscribe((rows) => {
        this.topLargestLandfills = rows.map((r) => ({
          name: r?.name ?? 'N/A',
          region:
            this.regionDisplayMap2[r?.regionTag?.toLowerCase() ?? ''] ??
            r?.regionTag ??
            'Unknown',
          totalWaste: Number(r?.estimatedAreaM2 ?? 0),
          areaM2: r?.estimatedAreaM2,
          yearCreated: r?.startYear,
        }));
        this.cdr.markForCheck();
      });

    // Most Impacted Region
    this.statisticsService
      .getMostImpactedRegion()
      .pipe(
        catchError(() =>
          of({
            region: 'Unknown',
            totalCh4: 0,
            totalCo2eq: 0,
            ch4PerKm2: 0,
            co2eqPerKm2: 0,
            population: 0,
            areaKm2: 0,
          } as MostImpactedView)
        )
      )
      .subscribe((mp) => {
        this.mostPollutedRegion = mp;
        this.cdr.markForCheck();
      });

    // Table
    this.landfillService
      .getAllLandfills()
      .pipe(
        catchError(() => of([])),
        map((landfills) =>
          (landfills ?? []).map((lf) => {
            const landfillName = lf?.name ?? `Landfill ${lf?.id ?? ''}`;
            const regionKey = lf?.regionTag?.toLowerCase?.().replace(/\s+/g, '') ?? '';
            const regionName =
              this.regionDisplayMap2[regionKey] ?? lf?.regionTag ?? 'Unknown';
            const year = this.toYear(lf?.startYear ?? lf?.createdAt);
            return {
              landfillName,
              regionName,
              year,
              volumeM3: Number(lf?.estimatedVolumeM3 ?? 0),
              wasteTons: Number(lf?.estimatedMSW ?? 0),
              ch4Tons: Number(lf?.estimatedCH4Tons ?? 0),
              co2eqTons: Number(lf?.estimatedCO2eTons ?? 0),
            };
          })
        )
      )
      .subscribe((rows: TableRow[]) => {
        this.dataSource = new MatTableDataSource<TableRow>(rows ?? []);
        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;
        this.cdr.markForCheck();
      });
  }

  // Shared chart style for all
  private getChartBaseOptions(): Partial<Highcharts.Options> {
    const isDark = localStorage.getItem('theme') === 'dark';
    const textColor = isDark ? '#e8e8e8' : '#333';
    const accentColor = isDark ? '#9ef49f' : '#006400';
    const bgColor = isDark ? 'rgb(42,42,42)' : '#ffffff';
    const gridColor = isDark ? 'rgba(255,255,255,0.1)' : '#e0e0e0';

    return {
      credits: { enabled: false },
      accessibility: { enabled: false },
      chart: {
        backgroundColor: bgColor,
        style: { color: textColor },
        borderRadius: 12,
        borderWidth: 0,
      },
      title: { text: undefined },
      legend: {
        itemStyle: { color: textColor },
        itemHoverStyle: { color: accentColor },
      },
      xAxis: {
        lineColor: gridColor,
        gridLineColor: gridColor,
        labels: { style: { color: textColor } },
        title: { style: { color: textColor } },
      },
      yAxis: {
        lineColor: gridColor,
        gridLineColor: gridColor,
        labels: { style: { color: textColor } },
        title: { style: { color: textColor } },
      },
      tooltip: {
        backgroundColor: isDark ? '#2c2c2c' : '#fff',
        borderColor: isDark ? '#444' : '#ccc',
        style: { color: textColor },
      },
    };
  }

  private toYear(v: unknown): number {
    if (!v) return NaN;
    const parsed = Number(v);
    if (Number.isFinite(parsed) && parsed > 1900 && parsed < 3000) return parsed;
    const d = new Date(v as string);
    return isNaN(d.getTime()) ? NaN : d.getFullYear();
  }

  applyFilter(value: string) {
    this.dataSource.filter = (value ?? '').trim().toLowerCase();
  }
}
