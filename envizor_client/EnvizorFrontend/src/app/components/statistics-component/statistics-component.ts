import {
  Component,
  OnInit,
  ViewChild,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';
import * as Highcharts from 'highcharts';
import { HighchartsChartComponent } from 'highcharts-angular';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import {
  StatisticsService,
  WasteByRegionDto,
  LandfillTypeDto,
  Ch4OverTimeDto,
  TopLandfillDto,
  MostImpactedRegionFullDto,
  LandfillService,
} from '../../services';
import { of, forkJoin } from 'rxjs';
import { takeUntil, catchError, map, switchMap } from 'rxjs/operators';
import { LandfillSite } from '../../DTOs/LandfillSiteAllDto';
import { SerbianRegion } from '../../DTOs/Enums/SerbianRegion';
// import { MonitoringService } from '../../services/monitoring-service';

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
  chartRegionWaste: Highcharts.Options = { accessibility: { enabled: false }, series: [] };
  chartSpeciesPie: Highcharts.Options = { accessibility: { enabled: false }, series: [] };
  chartEmissionsTrend: Highcharts.Options = { accessibility: { enabled: false }, series: [] };
  chartPrediction: Highcharts.Options = { accessibility: { enabled: false }, series: [] };

  topLargestLandfills: TopLandfillView[] = [];
  mostPollutedRegion?: MostImpactedView;
  chartRegionLandfills: Highcharts.Options = { accessibility: { enabled: false }, series: [] };

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
  regionDisplayMap: Record<SerbianRegion, string> = {
    [SerbianRegion.Vojvodina]: 'Vojvodina',
    [SerbianRegion.Belgrade]: 'Beograd',
    [SerbianRegion.WesternSerbia]: 'Zapadna Srbija',
    [SerbianRegion.SumadijaAndPomoravlje]: 'Šumadija i Pomoravlje',
    [SerbianRegion.EasternSerbia]: 'Istočna Srbija',
    [SerbianRegion.SouthernSerbia]: 'Južna Srbija',
    [SerbianRegion.KosovoAndMetohija]: 'Kosovo i Metohija',
  };
  // In your component, already defined:
  regionDisplayMap2: Record<string, string> = {
    beograd: 'Beograd',
    vojvodina: 'Vojvodina',
    zapadnasrbija: 'Zapadna Srbija',
    sumadijapomoravlje: 'Šumadija i Pomoravlje',
    istocnasrbija: 'Istočna Srbija',
    juznasrbija: 'Južna Srbija',
    // Add all possible regionTag values here
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  constructor(
    private statisticsService: StatisticsService,
    private landfillService: LandfillService,
    // private monitoringService: MonitoringService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Chart 1: Total waste by region
    this.landfillService
      .getAllLandfills()
      .pipe(
        map((landfills: LandfillSite[]) => {
          const regionStats: Record<string, number> = {};
          for (const lf of landfills) {
            const regionTag = lf.regionTag;
            if (!regionTag) continue; // skip if missing
            // If you want to map to display names:
            // const displayRegion = regionDisplayMap[regionTag as SerbianRegion] || regionTag;
            // regionStats[displayRegion] = (regionStats[displayRegion] || 0) + Number(lf.estimatedMSW ?? 0);
            // If regionTag is already display name, use directly:
            regionStats[regionTag] = (regionStats[regionTag] || 0) + Number(lf.estimatedMSW ?? 0);
          }
          return Object.entries(regionStats)
            .sort(([a], [b]) => a.localeCompare(b))
            .map(([name, waste]) => ({ name, totalWaste: waste }));
        }),
        catchError(() => of([]))
      )
      .subscribe((rows) => {
        this.chartRegionWaste = {
          accessibility: { enabled: false },
          chart: { type: 'column' },
          xAxis: {
            categories: rows.map((r) => r.name),
            title: { text: 'Region' },
            labels: { rotation: -45 },
          },
          yAxis: {
            title: { text: 'Waste (t)' },
            min: 0,
          },
          tooltip: {
            pointFormat: '{series.name}: <b>{point.y:.0f} t</b>',
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
    // Chart 2: Landfill types (pie)
    this.landfillService
      .getAllLandfills()
      .pipe(
        map((landfills: LandfillSite[]) => {
          // Count landfills by type
          const typeCounts: Record<string, number> = {};
          for (const lf of landfills) {
            // Convert enum value to string label
            let typeLabel = '';
            switch (lf.category) {
              case 0:
                typeLabel = 'Illegal';
                break;
              case 1:
                typeLabel = 'NonSanitary';
                break;
              case 2:
                typeLabel = 'Sanitary';
                break;
              default:
                typeLabel = 'Unknown';
            }
            typeCounts[typeLabel] = (typeCounts[typeLabel] || 0) + 1;
          }
          // Convert to chart data format
          return Object.entries(typeCounts).map(([name, count]) => ({ name, y: count }));
        }),
        catchError(() => of([]))
      )
      .subscribe((data) => {
        this.chartSpeciesPie = {
          accessibility: { enabled: false },
          chart: { type: 'pie' },
          series: [
            {
              type: 'pie',
              name: 'Count',
              data: data,
            },
          ],
        };
        this.cdr.markForCheck();
      });

    // Chart 3: CH4 emissions over time & prediction
    this.statisticsService
      .getCh4EmissionsOverTime()
      .pipe(
        catchError(() => of({ years: [], ch4ByYear: [] } as Ch4OverTimeDto)),
        map((res) => {
          const years = Array.isArray(res?.years) ? res.years.map(Number) : [];
          const ch4 = Array.isArray(res?.ch4ByYear) ? res.ch4ByYear.map(Number) : [];
          return { years, ch4 };
        })
      )
      .subscribe(({ years, ch4 }) => {
        this.chartEmissionsTrend = {
          accessibility: { enabled: false },
          chart: { type: 'line' },
          xAxis: { categories: years.map(String) },
          yAxis: { title: { text: 'CH₄ (t)' } },
          series: [{ type: 'line', name: 'CH₄', data: ch4 }],
        };
        // Prediction chart logic can be added here if needed
        this.cdr.markForCheck();
      });

    // Top 3 largest landfills
    this.landfillService
      .getAllLandfills()
      .pipe(
        map((landfills: LandfillSite[]) => {
          // Sort by estimatedAreaM2 descending and take top 3
          return (landfills ?? [])
            .filter((lf) => typeof lf.estimatedAreaM2 === 'number')
            .sort((a, b) => (b.estimatedAreaM2 ?? 0) - (a.estimatedAreaM2 ?? 0))
            .slice(0, 3);
        }),
        catchError(() => of([] as LandfillSite[]))
      )
      .subscribe((rows) => {
        this.topLargestLandfills = rows.map((r) => ({
          name: r?.name ?? 'N/A',
          region:
            this.regionDisplayMap2[r?.regionTag?.toLowerCase() ?? ''] ?? r?.regionTag ?? 'Unknown',
          totalWaste: Number(r?.estimatedAreaM2 ?? 0),
          areaM2: r?.estimatedAreaM2,
          yearCreated: r?.startYear,
        }));
        this.cdr.markForCheck();
      });
    // Most impacted region
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
          } as MostImpactedRegionFullDto)
        )
      )
      .subscribe((mp) => {
        this.mostPollutedRegion = mp;
        this.cdr.markForCheck();
      });

    // Table: all landfills (replace with your actual data logic)
    this.landfillService
      .getAllLandfills()
      .pipe(
        // takeUntil(this.destroy$),
        catchError(() => of([])), // no landfills = empty table
        switchMap((landfills) => {
          if (!Array.isArray(landfills) || landfills.length === 0) {
            return of([] as TableRow[]);
          }

          // For each landfill, get its latest monitoring (in parallel)
          const perLandfill$ = landfills.map((lf: any) =>
            this.landfillService.getAllLandfills().pipe(
              catchError(() => of(null)),
              map((mon: any | null): TableRow => {
                // Prefer monitoring values; fall back to landfill estimates
                const landfillName = lf?.name ?? `Landfill ${lf?.id ?? ''}`;
                const regionName = lf?.region ?? '—';

                const year =
                  mon?.year ??
                  this.toYear(mon?.timestamp || mon?.date || mon?.measuredAt || 0) ??
                  this.toYear(lf?.startYear || 0);

                const volumeM3 = Number(mon?.estimatedVolumeM3 ?? lf?.estimatedVolumeM3 ?? 0);
                const wasteTons = Number(mon?.estimatedWasteTons ?? lf?.estimatedMSW ?? 0);
                const ch4Tons = Number(mon?.estimatedCH4Tons ?? lf?.estimatedCH4Tons ?? 0);
                // If CO₂eq not present, approximate from CH₄ using GWP100 ≈ 28
                const co2eqTons = Number(
                  mon?.co2eqTons ?? (Number.isFinite(ch4Tons) ? ch4Tons * 28 : 0)
                );

                return {
                  landfillName,
                  regionName,
                  year: this.toYear(year),
                  volumeM3: Number.isFinite(volumeM3) ? volumeM3 : 0,
                  wasteTons: Number.isFinite(wasteTons) ? wasteTons : 0,
                  ch4Tons: Number.isFinite(ch4Tons) ? ch4Tons : 0,
                  co2eqTons: Number.isFinite(co2eqTons) ? co2eqTons : 0,
                };
              })
            )
          );

          return forkJoin(perLandfill$);
        })
      )
      .subscribe((rows: TableRow[]) => {
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

        // Filter across all columns
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

        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;

        this.cdr.markForCheck();
      });
  }
  private toYear(v: unknown, fallback = 2024): number {
    const n = Number(v);
    if (Number.isFinite(n) && n > 0 && n < 3000) return n;
    if (typeof v === 'string') {
      const d = new Date(v);
      if (!isNaN(d.getTime())) return d.getFullYear();
    }
    return fallback;
  }

  applyFilter(value: string) {
    this.dataSource.filter = (value ?? '').trim().toLowerCase();
  }
}
