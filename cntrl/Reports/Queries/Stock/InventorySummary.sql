﻿SELECT 
ii.trans_date as InventoryDate, 
u.name_full as UserName,
i.code as ItemCode, 
i.name as ItemName, 
sum(iid.value_system) as SystemQuantity,
sum(iid.value_counted) as CountedQuantity, (sum(iid.value_counted) - 
sum(iid.value_system)) as Difference,
iid.unit_value as ItemCost, 
(sum(iid.value_counted) - sum(iid.value_system)) * iid.unit_value as TotalCost,


iid.comment as Comment,
branch.name as Branch,
location.name as Location  

FROM item_inventory_detail iid inner join item_inventory as ii on iid.id_inventory=ii.id_inventory
inner join item_product as ip  on iid.id_item_product=ip.id_item_product 
inner join app_branch as branch on ii.id_branch=branch.id_branch 
inner join app_location as location on iid.id_location=location.id_location
left join items as i on ip.id_item = i.id_item
 
 inner join security_user as u on ii.id_user = u.id_user

 
where ii.status = 1 and (ii.trans_date >= @StartDate) AND (ii.trans_date <= @EndDate) and iid.id_company = @CompanyID 
group by iid.id_item_product
order by i.name