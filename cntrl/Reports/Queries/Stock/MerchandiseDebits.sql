﻿
select branch.name as BranchName,
inv.comment as TransComment,
item.code as ItemCode,
item.name as ItemName,
inv.debit as Credit,
UnitCost,
                (UnitCost* inv.debit) as TotalCost,
                inv.trans_date as TransDate

              from(
              select item_movement.*, sum(val.unit_value) as UnitCost
              from item_movement
              left outer join item_movement_value as val on item_movement.id_movement = val.id_movement
              where item_movement.id_company = {0} and item_movement.trans_date between '{1}' and '{2}' 
              and (
                    item_movement.id_sales_invoice_detail > 0 or
                    item_movement.id_execution_detail > 0 or
                    item_movement.id_inventory_detail > 0 or
                    item_movement.id_transfer_detail > 0)

              group by item_movement.id_movement
                ) as inv

              inner join item_product as prod on inv.id_item_product = prod.id_item_product
              inner join items as item on prod.id_item = item.id_item
              inner join app_location as loc on inv.id_location = loc.id_location
              inner join app_branch as branch on loc.id_branch = branch.id_branch

              group by inv.id_movement
              order by inv.trans_date