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
  Ch4OverTimeDto,
  MostImpactedRegionFullDto,
} from '../../services';
import { RegionService, LandfillService, StatisticsService } from '../../services';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { LandfillSite } from '../../DTOs';
import { SerbianRegion } from '../../DTOs/Enums/SerbianRegion';

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
    private regionService: RegionService,
    // private monitoringService: MonitoringService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Chart 1: Total waste by region
    this.regionService
      .getAllRegions()
      .subscribe((rows) => {
        this.chartRegionWaste = {
          title: { text: undefined },
          credits: { enabled: false },
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
          title: { text: undefined },
          credits: { enabled: false },
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
          title: { text: undefined },
          credits: { enabled: false },
          accessibility: { enabled: false },
          chart: { type: 'line' },
          xAxis: { categories: years.map(String) },
          yAxis: { title: { text: 'CH₄ (t)' } },
          series: [{ type: 'line', name: 'CH₄', data: ch4 }],
        };
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
        catchError(() => of([])), // no landfills = empty table
        map((landfills) => {
          return (landfills ?? []).map((lf) => {
            const landfillName = lf?.name ?? `Landfill ${lf?.id ?? ''}`;

            const regionKey = lf?.regionTag?.toLowerCase?.().replace(/\s+/g, '') ?? '';
            const regionName =
              this.regionDisplayMap2[regionKey] ?? lf?.regionTag ?? 'Unknown';

            const year = this.toYear(lf?.startYear ?? lf?.createdAt);
            const volumeM3 = Number(lf?.estimatedVolumeM3 ?? 0);
            const wasteTons = Number(lf?.estimatedMSW ?? 0);
            const ch4Tons = Number(lf?.estimatedCH4Tons ?? 0);
            const co2eqTons = Number(lf?.estimatedCO2eTons ?? 0);

            return {
              landfillName,
              regionName,
              year,
              volumeM3,
              wasteTons,
              ch4Tons,
              co2eqTons,
            };
          });
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